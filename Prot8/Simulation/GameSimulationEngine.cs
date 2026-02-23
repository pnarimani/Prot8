using Prot8.Constants;
using Prot8.Events;
using Prot8.Jobs;
using Prot8.Laws;
using Prot8.Missions;
using Prot8.Orders;
using Prot8.Resources;
using Prot8.Zones;

namespace Prot8.Simulation;

public sealed class GameSimulationEngine(GameState state)
{
    public static void RollDailyDisruption(GameState state)
    {
        state.ActiveDisruption = null;
        state.DailyEffects = new TemporaryDailyEffects();

        if (state.Day < 3)
        {
            return;
        }

        if (state.RollPercent() > 25)
        {
            return;
        }

        var roll = state.Random.Next(0, 5);
        switch (roll)
        {
            case 0:
                state.ActiveDisruption = "Heavy Rains: Food production -30%, water production +30% today.";
                state.DailyEffects.FoodProductionMultiplier *= 0.7;
                state.DailyEffects.WaterProductionMultiplier *= 1.3;
                break;
            case 1:
                state.ActiveDisruption = "Cold Snap: Fuel consumption +50% today, +2 sickness.";
                state.DailyEffects.FuelConsumptionMultiplier = 1.5;
                break;
            case 2:
                state.ActiveDisruption = "Dust Storm: Materials crafting -40%, siege damage -20% today.";
                state.DailyEffects.MaterialsProductionMultiplier *= 0.6;
                state.DailyEffects.DustStormActive = true;
                break;
            case 3:
                state.ActiveDisruption = "Clear Skies: All production +15% today.";
                state.DailyEffects.ProductionMultiplier *= 1.15;
                break;
            case 4:
                state.ActiveDisruption = "Fog Cover: Missions +10% success, repairs -30% today.";
                state.DailyEffects.MissionSuccessBonus = 0.10;
                state.DailyEffects.RepairProductionMultiplier *= 0.7;
                break;
        }
    }

    public DayResolutionReport ResolveDay(TurnActionChoice action)
    {
        var report = new DayResolutionReport(state.Day);
        report.StartFood = state.Resources[Resources.ResourceKind.Food];
        report.StartWater = state.Resources[Resources.ResourceKind.Water];
        report.StartFuel = state.Resources[Resources.ResourceKind.Fuel];
        report.StartMorale = state.Morale;
        report.StartUnrest = state.Unrest;
        report.StartSickness = state.Sickness;
        report.StartHealthyWorkers = state.Population.HealthyWorkers;

        PrepareDay(state);
        ApplyPlayerAction(state, action, report);

        ApplyLawPassives(state, report);
        ApplyEmergencyOrderEffects(state, report);

        var production = CalculateProduction(state, report);
        ApplyConsumption(state, report);
        ApplyDeficitPenalties(state, report);
        ApplyOvercrowdingPenalties(state, report);
        ApplySicknessProgression(state, production, report);
        ApplyUnrestProgression(state, report);
        ApplySiegeDamage(state, report);
        ApplyRepairs(state, production, report);
        ResolveTriggeredEvents(state, report);
        ResolveActiveMissions(state, report);
        CheckLossConditions(state, report);

        FinalizeDay(state, report);
        return report;
    }

    static void PrepareDay(GameState state)
    {
        state.ActiveOrderId = null;
        state.FoodDeficitToday = false;
        state.WaterDeficitToday = false;
        state.FuelDeficitToday = false;

        if (state.SiegeDamageReductionDaysRemaining > 0)
        {
            state.SiegeDamageReductionDaysRemaining -= 1;
            if (state.SiegeDamageReductionDaysRemaining <= 0)
            {
                state.SiegeDamageMultiplier = 1.0;
            }
        }

        if (state.TaintedWellDaysRemaining > 0)
        {
            state.DailyEffects.WaterProductionMultiplier = 0.6;
            state.TaintedWellDaysRemaining -= 1;
        }
    }

    static void ApplyPlayerAction(GameState state, TurnActionChoice action, DayResolutionReport report)
    {
        if (!string.IsNullOrWhiteSpace(action.LawId))
        {
            var law = LawCatalog.Find(action.LawId);
            if (law is null)
            {
                report.Add(ReasonTags.LawEnact, "Law selection failed: law not found.");
                return;
            }

            if (state.ActiveLawIds.Contains(law.Id))
            {
                report.Add(ReasonTags.LawEnact, $"Law already enacted: {law.Name}.");
                return;
            }

            var lawCooldownActive = state.LastLawDay != int.MinValue
                                    && state.Day - state.LastLawDay < GameBalance.LawCooldownDays;
            if (lawCooldownActive)
            {
                var nextDay = state.LastLawDay + GameBalance.LawCooldownDays;
                report.Add(ReasonTags.LawEnact, $"Law cooldown active. Next enactment day: {nextDay}.");
                return;
            }

            if (!law.CanEnact(state, out var reason))
            {
                report.Add(ReasonTags.LawEnact, $"Cannot enact {law.Name}: {reason}");
                return;
            }

            state.ActiveLawIds.Add(law.Id);
            state.LastLawDay = state.Day;
            if (!state.FirstLawDay.HasValue)
            {
                state.FirstLawDay = state.Day;
                state.FirstLawName = law.Name;
            }

            law.OnEnact(state, report);
            report.Add(ReasonTags.LawEnact, $"Law enacted: {law.Name}.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(action.EmergencyOrderId))
        {
            var order = EmergencyOrderCatalog.Find(action.EmergencyOrderId);
            if (order is null)
            {
                report.Add(ReasonTags.OrderEffect, "Emergency order selection failed: order not found.");
                return;
            }

            if (state.OrderCooldowns.TryGetValue(order.Id, out var lastDay)
                && state.Day - lastDay < order.CooldownDays)
            {
                var nextDay = lastDay + order.CooldownDays;
                report.Add(ReasonTags.OrderEffect, $"Emergency order cooldown active for {order.Name}. Next available day: {nextDay}.");
                return;
            }

            if (!order.CanIssue(state, out var reason))
            {
                report.Add(ReasonTags.OrderEffect, $"Cannot issue {order.Name}: {reason}");
                return;
            }

            state.ActiveOrderId = order.Id;
            state.OrderCooldowns[order.Id] = state.Day;
            report.Add(ReasonTags.OrderEffect, $"Emergency order prepared: {order.Name}.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(action.MissionId))
        {
            var mission = MissionCatalog.Find(action.MissionId);
            if (mission is null)
            {
                report.Add(ReasonTags.Mission, "Mission selection failed: mission not found.");
                return;
            }

            if (state.MissionCooldowns.TryGetValue(mission.Id, out var lastMissionDay))
            {
                if (state.Day - lastMissionDay < GameBalance.MissionCooldownDays)
                {
                    var nextDay = lastMissionDay + GameBalance.MissionCooldownDays;
                    report.Add(ReasonTags.Mission, $"Mission cooldown active for {mission.Name}. Next available day: {nextDay}.");
                    return;
                }
            }

            if (!mission.CanStart(state, out var reason))
            {
                report.Add(ReasonTags.Mission, $"Cannot start mission {mission.Name}: {reason}");
                return;
            }

            if (state.IdleWorkers < mission.WorkerCost)
            {
                report.Add(ReasonTags.Mission,
                    $"Cannot start mission {mission.Name}: not enough idle workers (need {mission.WorkerCost}, have {state.IdleWorkers}).");
                return;
            }

            state.ActiveMissions.Add(new ActiveMission(mission.Id, mission.Name, mission.DurationDays,
                mission.WorkerCost));
            state.MissionCooldowns[mission.Id] = state.Day;
            report.Add(ReasonTags.Mission,
                $"Mission started: {mission.Name} ({mission.DurationDays} day(s), {mission.WorkerCost} workers committed).");
        }
    }

    static void ApplyLawPassives(GameState state, DayResolutionReport report)
    {
        foreach (var lawId in state.ActiveLawIds)
        {
            var law = LawCatalog.Find(lawId);
            law?.ApplyDaily(state, report);
        }
    }

    static void ApplyEmergencyOrderEffects(GameState state, DayResolutionReport report)
    {
        if (string.IsNullOrWhiteSpace(state.ActiveOrderId))
        {
            return;
        }

        var order = EmergencyOrderCatalog.Find(state.ActiveOrderId);
        if (order is null)
        {
            return;
        }

        order.Apply(state, report);
    }

    static DailyProductionResult CalculateProduction(GameState state, DayResolutionReport report)
    {
        var result = new DailyProductionResult();

        var globalMultiplier = StatModifiers.ComputeGlobalProductionMultiplier(state) *
                               state.DailyEffects.ProductionMultiplier;

        foreach (var job in Enum.GetValues<JobType>())
        {
            var workers = state.Allocation.Workers[job];
            if (workers <= 0)
            {
                continue;
            }

            var zoneMultiplier = 1.0;
            if (job != JobType.Repairs)
            {
                var mappedZone = GameBalance.JobZoneMap[job];
                if (state.IsZoneLost(mappedZone))
                {
                    zoneMultiplier *= GameBalance.LostZoneJobMultipliers[job];
                }

                if (state.DailyEffects.QuarantineZone.HasValue && state.DailyEffects.QuarantineZone.Value == mappedZone)
                {
                    zoneMultiplier *= 0.5;
                }
            }

            if (job == JobType.WaterDrawing)
                zoneMultiplier *= state.DailyEffects.WaterProductionMultiplier;
            else if (job == JobType.FoodProduction)
                zoneMultiplier *= state.DailyEffects.FoodProductionMultiplier;
            else if (job == JobType.MaterialsCrafting)
                zoneMultiplier *= state.DailyEffects.MaterialsProductionMultiplier;
            else if (job == JobType.Repairs)
                zoneMultiplier *= state.DailyEffects.RepairProductionMultiplier;

            var nominalCycles = workers * globalMultiplier * zoneMultiplier;
            if (nominalCycles <= 0)
            {
                continue;
            }

            var scale = 1.0;
            var inputs = GameBalance.JobInputs[job];
            var hasInput = inputs.Count > 0;
            if (hasInput)
            {
                foreach (var pair in inputs)
                {
                    var perCycle = pair.Quantity;
                    if (job == JobType.ClinicStaff && pair.Resource == ResourceKind.Medicine)
                    {
                        perCycle *= state.DailyEffects.MedicineUsageMultiplier;
                    }

                    if (perCycle <= 0)
                    {
                        continue;
                    }

                    var needed = nominalCycles * perCycle;
                    var available = state.Resources[pair.Resource];
                    if (needed > available)
                    {
                        var candidate = available / needed;
                        if (candidate < scale)
                        {
                            scale = candidate;
                        }
                    }
                }
            }

            if (scale < 0)
            {
                scale = 0;
            }

            var effectiveCycles = nominalCycles * scale;
            if (effectiveCycles <= 0)
            {
                continue;
            }

            if (hasInput)
            {
                foreach (var pair in inputs)
                {
                    var perCycle = pair.Quantity;
                    if (job == JobType.ClinicStaff && pair.Resource == ResourceKind.Medicine)
                    {
                        perCycle *= state.DailyEffects.MedicineUsageMultiplier;
                    }

                    var spend = Math.Min((int)Math.Ceiling(effectiveCycles * perCycle), state.Resources[pair.Resource]);
                    if (spend > 0)
                    {
                        state.Resources.Consume(pair.Resource, spend);
                        report.Add(ReasonTags.Production, $"{job}: consumed {spend} {pair.Resource}.");
                        if (job == JobType.ClinicStaff && pair.Resource == ResourceKind.Medicine)
                        {
                            result.ClinicMedicineSpent += spend;
                        }
                    }
                }
            }

            var outputs = GameBalance.JobOutputs[job];
            var outputResource = outputs.FirstOrDefault();
            var produced = (int)Math.Floor(effectiveCycles * outputResource.Quantity);
            if (produced <= 0)
            {
                continue;
            }

            if (outputResource.Resource is not ResourceKind.Integrity and not ResourceKind.Care)
            {
                state.Resources.Add(outputResource.Resource, produced);
                result.AddResourceProduction(outputResource.Resource, produced);
                report.Add(ReasonTags.Production, $"{job}: +{produced} {outputResource.Resource}.");
            }
            else if (job == JobType.Repairs)
            {
                result.RepairPoints += produced;
                report.Add(ReasonTags.Production, $"Repair teams prepared {produced} integrity points.");
            }
            else if (job == JobType.ClinicStaff)
            {
                result.ClinicCarePoints += produced;
                result.ClinicSlotsUsed += (int)Math.Max(1, Math.Floor(effectiveCycles));
                report.Add(ReasonTags.Production, $"Clinic care capacity: {produced} care points.");
            }
        }

        return result;
    }

    static void ApplyConsumption(GameState state, DayResolutionReport report)
    {
        var population = state.Population.TotalPopulation;

        var foodNeed = (int)Math.Ceiling(population * GameBalance.FoodPerPersonPerDay *
                                         state.DailyEffects.FoodConsumptionMultiplier);
        var waterNeed = (int)Math.Ceiling(population * GameBalance.WaterPerPersonPerDay *
                                          state.DailyEffects.WaterConsumptionMultiplier);
        var fuelNeed = (int)Math.Ceiling(population * GameBalance.FuelPerPersonPerDay *
                                         state.DailyEffects.FuelConsumptionMultiplier);

        var foodConsumed = state.Resources.Consume(ResourceKind.Food, foodNeed);
        var waterConsumed = state.Resources.Consume(ResourceKind.Water, waterNeed);
        var fuelConsumed = state.Resources.Consume(ResourceKind.Fuel, fuelNeed);

        report.FoodConsumedToday = foodConsumed;
        report.WaterConsumedToday = waterConsumed;
        state.LastDayFoodConsumed = foodConsumed;
        state.LastDayWaterConsumed = waterConsumed;

        report.Add(ReasonTags.Consumption,
            $"Daily consumption: food {foodConsumed}/{foodNeed}, water {waterConsumed}/{waterNeed}, fuel {fuelConsumed}/{fuelNeed}.");

        if (foodConsumed < foodNeed)
        {
            state.FoodDeficitToday = true;
            report.FoodDeficitToday = true;
            report.Add(ReasonTags.Consumption, $"Food deficit: short by {foodNeed - foodConsumed}.");
        }

        if (waterConsumed < waterNeed)
        {
            state.WaterDeficitToday = true;
            report.WaterDeficitToday = true;
            report.Add(ReasonTags.Consumption, $"Water deficit: short by {waterNeed - waterConsumed}.");
        }

        if (fuelConsumed < fuelNeed)
        {
            state.FuelDeficitToday = true;
            report.FuelDeficitToday = true;
            report.Add(ReasonTags.Consumption, $"Fuel deficit: short by {fuelNeed - fuelConsumed}.");
        }
    }

    static void ApplyDeficitPenalties(GameState state, DayResolutionReport report)
    {
        if (state.FoodDeficitToday)
        {
            if (!state.DayFirstFoodDeficit.HasValue)
            {
                state.DayFirstFoodDeficit = state.Day;
            }

            state.ConsecutiveFoodDeficitDays += 1;
            StateChangeApplier.AddUnrest(state, 6, report, ReasonTags.Deficit, "Food deficit pressure");
            StateChangeApplier.AddMorale(state, -8, report, ReasonTags.Deficit, "Food deficit pressure");
            StateChangeApplier.AddSickness(state, 2, report, ReasonTags.Deficit, "Food deficit pressure");
        }
        else
        {
            state.ConsecutiveFoodDeficitDays = 0;
        }

        if (state.WaterDeficitToday)
        {
            if (!state.DayFirstWaterDeficit.HasValue)
            {
                state.DayFirstWaterDeficit = state.Day;
            }

            state.ConsecutiveWaterDeficitDays += 1;
            StateChangeApplier.AddUnrest(state, 7, report, ReasonTags.Deficit, "Water deficit pressure");
            StateChangeApplier.AddMorale(state, -10, report, ReasonTags.Deficit, "Water deficit pressure");
            StateChangeApplier.AddSickness(state, 6, report, ReasonTags.Deficit, "Water deficit pressure");
        }
        else
        {
            state.ConsecutiveWaterDeficitDays = 0;
        }

        if (state.FuelDeficitToday)
        {
            StateChangeApplier.AddMorale(state, -4, report, ReasonTags.Deficit, "Fuel shortage");
            StateChangeApplier.AddSickness(state, 2, report, ReasonTags.Deficit, "Fuel shortage");
        }
    }

    static void ApplyOvercrowdingPenalties(GameState state, DayResolutionReport report)
    {
        var totalPop = state.Population.TotalPopulation;
        var totalCapacity = 0;
        foreach (var zone in state.Zones)
        {
            if (!zone.IsLost)
                totalCapacity += zone.Capacity;
        }

        var overflow = totalPop - totalCapacity;
        var totalStacks = overflow > 0 ? overflow / GameBalance.OvercrowdingThreshold : 0;

        if (totalStacks <= 0)
        {
            report.OvercrowdingStacksToday = 0;
            return;
        }

        report.OvercrowdingStacksToday = totalStacks;

        StateChangeApplier.AddUnrest(state, totalStacks * GameBalance.OvercrowdingUnrestPerStack, report,
            ReasonTags.Overcrowding, "Overcrowding");
        StateChangeApplier.AddSickness(state, totalStacks * GameBalance.OvercrowdingSicknessPerStack, report,
            ReasonTags.Overcrowding, "Overcrowding");

        var multiplier = totalStacks * GameBalance.OvercrowdingConsumptionPerStack;
        var extraFood = (int)Math.Ceiling(report.FoodConsumedToday * multiplier);
        var extraWater = (int)Math.Ceiling(report.WaterConsumedToday * multiplier);

        if (extraFood > 0)
        {
            var consumed = state.Resources.Consume(ResourceKind.Food, extraFood);
            report.Add(ReasonTags.Overcrowding,
                $"Overcrowding strain: additional food consumption {consumed}/{extraFood}.");
        }

        if (extraWater > 0)
        {
            var consumed = state.Resources.Consume(ResourceKind.Water, extraWater);
            report.Add(ReasonTags.Overcrowding,
                $"Overcrowding strain: additional water consumption {consumed}/{extraWater}.");
        }
    }

    static void ApplySicknessProgression(GameState state, DailyProductionResult production, DayResolutionReport report)
    {
        var sicknessDelta = StatModifiers.ComputeSicknessFromEnvironment(state);
        sicknessDelta -= state.DailyEffects.QuarantineSicknessReduction;
        sicknessDelta -= production.ClinicCarePoints / 3;

        if (state.DailyEffects.FuelConsumptionMultiplier > 1.0)
        {
            sicknessDelta += 2;
        }

        if (sicknessDelta != 0)
        {
            StateChangeApplier.AddSickness(state, sicknessDelta, report, ReasonTags.Sickness,
                "Daily sickness progression");
        }

        var newCases = Math.Max(0, (state.Sickness - 15) / 20);
        if (state.FoodDeficitToday)
        {
            newCases += 1;
        }

        if (state.WaterDeficitToday)
        {
            newCases += 2;
        }

        newCases -= production.ClinicCarePoints / 10;
        if (newCases > 0)
        {
            StateChangeApplier.ConvertHealthyToSick(state, newCases, report, "Disease spread");
        }

        var diseaseDeaths = state.Sickness > 70 ? Math.Max(1, (state.Sickness - 70) / 15) : 0;
        if (diseaseDeaths > 0)
        {
            StateChangeApplier.ApplyDeaths(state, diseaseDeaths, report, ReasonTags.Sickness,
                "Critical sickness mortality");
        }

        if (state.Sickness < GameBalance.RecoveryThresholdSickness)
        {
            report.RecoveryEnabledToday = true;
            state.Population.AdvanceRecoveryTimers();
            report.Add(ReasonTags.RecoveryProgress, "Recovery timers advanced.");

            var ready = state.Population.ReadyToRecoverCount();
            var clinicCap = production.ClinicSlotsUsed * GameBalance.RecoveryPerClinicSlot;
            var medicinePerRecovery = GameBalance.MedicinePerRecovery * state.DailyEffects.MedicineUsageMultiplier;
            var medicineCap = medicinePerRecovery <= 0
                ? ready
                : (int)Math.Floor(state.Resources[ResourceKind.Medicine] / medicinePerRecovery);

            var recoveries = Math.Min(ready, Math.Min(clinicCap, medicineCap));
            if (recoveries > 0)
            {
                var medicineSpend = (int)Math.Ceiling(recoveries * medicinePerRecovery);
                state.Resources.Consume(ResourceKind.Medicine, medicineSpend);
                report.RecoveryMedicineSpentToday += medicineSpend;
                StateChangeApplier.RecoverSickWorkers(state, recoveries, report, "Clinical recovery");
            }
            else if (ready > 0)
            {
                if (clinicCap <= 0)
                {
                    report.RecoveryBlockedReason = "No clinic staff assigned.";
                }
                else if (medicineCap <= 0)
                {
                    report.RecoveryBlockedReason = "Insufficient medicine for recoveries.";
                    report.Add(ReasonTags.RecoveryBlockedMedicine, "Recovery blocked by medicine shortage.");
                }
            }
        }
        else
        {
            report.RecoveryEnabledToday = false;
            report.RecoveryBlockedReason =
                $"Global sickness is {state.Sickness}, recovery requires below {GameBalance.RecoveryThresholdSickness}.";
            report.Add(ReasonTags.RecoveryBlockedThreshold, report.RecoveryBlockedReason);
        }
    }

    static void ApplyUnrestProgression(GameState state, DayResolutionReport report)
    {
        var unrestDelta = StatModifiers.ComputeUnrestProgression(state);
        if (state.FoodDeficitToday)
        {
            unrestDelta += 2;
        }

        if (state.WaterDeficitToday)
        {
            unrestDelta += 2;
        }

        if (unrestDelta != 0)
        {
            StateChangeApplier.AddUnrest(state, unrestDelta, report, ReasonTags.Unrest, "Daily unrest progression");
        }

        var moraleDelta = StatModifiers.ComputeMoraleDrift(state);
        if (moraleDelta != 0)
        {
            StateChangeApplier.AddMorale(state, moraleDelta, report, ReasonTags.Unrest, "Daily morale drift");
        }
    }

    static void ApplySiegeDamage(GameState state, DayResolutionReport report)
    {
        if (state.SiegeEscalationDelayDays > 0)
        {
            state.SiegeEscalationDelayDays -= 1;
            report.Add(ReasonTags.Siege,
                $"Night raid pressure delay active: {state.SiegeEscalationDelayDays} day(s) remaining.");
        }
        else
        {
            var shouldEscalate = state.Day % GameBalance.SiegeEscalationIntervalDays == 0;
            var pressureFlags = 0;

            if (state.Unrest >= 65)
            {
                pressureFlags += 1;
            }

            if (state.Sickness >= 60)
            {
                pressureFlags += 1;
            }

            if (state.ConsecutiveFoodDeficitDays > 0)
            {
                pressureFlags += 1;
            }

            if (state.ConsecutiveWaterDeficitDays > 0)
            {
                pressureFlags += 1;
            }

            if (state.CountLostZones() >= 2)
            {
                pressureFlags += 1;
            }

            if (pressureFlags >= 3)
            {
                shouldEscalate = true;
            }

            if (shouldEscalate && state.SiegeIntensity < GameBalance.MaxSiegeIntensity)
            {
                state.SiegeIntensity += 1;
                report.Add(ReasonTags.Siege, $"Siege intensity increased to {state.SiegeIntensity}.");
            }
        }

        var perimeter = state.ActivePerimeterZone;
        var perimeterFactor = ZoneRules.PerimeterFactor(state);
        var finalAssaultMultiplier = state.FinalAssaultActive ? 1.5 : 1.0;
        var dustStormMultiplier = state.DailyEffects.DustStormActive ? 0.8 : 1.0;
        var damage = (int)Math.Ceiling((GameBalance.PerimeterScalingBase + state.SiegeIntensity) * perimeterFactor *
                                       state.SiegeDamageMultiplier * finalAssaultMultiplier * dustStormMultiplier);

        perimeter.Integrity -= damage;
        report.Add(ReasonTags.Siege, $"Siege struck {perimeter.Name}: -{damage} integrity.");

        if (perimeter.Integrity <= 0)
        {
            StateChangeApplier.LoseZone(state, perimeter.Id, false, report);
        }

        if (state.SiegeIntensity >= 4 && state.RollPercent() <= 15)
        {
            var foodRaid = (int)Math.Ceiling(state.Resources[Resources.ResourceKind.Food] * 0.15);
            var waterRaid = (int)Math.Ceiling(state.Resources[Resources.ResourceKind.Water] * 0.15);
            if (foodRaid > 0)
                StateChangeApplier.AddResource(state, Resources.ResourceKind.Food, -foodRaid, report, ReasonTags.Siege, "Supply line raid");
            if (waterRaid > 0)
                StateChangeApplier.AddResource(state, Resources.ResourceKind.Water, -waterRaid, report, ReasonTags.Siege, "Supply line raid");
            if (foodRaid > 0 || waterRaid > 0)
                report.Add(ReasonTags.Siege, $"Supply line raid: enemy forces destroyed {foodRaid} food and {waterRaid} water.");
        }
    }

    static void ApplyRepairs(GameState state, DailyProductionResult production, DayResolutionReport report)
    {
        var repairPoints = (int)Math.Round(production.RepairPoints * state.DailyEffects.RepairOutputMultiplier);
        if (repairPoints <= 0)
        {
            return;
        }

        var perimeter = state.ActivePerimeterZone;
        if (perimeter.IsLost)
        {
            return;
        }

        var before = perimeter.Integrity;
        perimeter.Integrity = Math.Min(100, perimeter.Integrity + repairPoints);
        var applied = perimeter.Integrity - before;

        if (applied > 0)
        {
            report.Add(ReasonTags.Repairs, $"Repair crews restored {applied} integrity to {perimeter.Name}.");
        }
    }

    static void ResolveTriggeredEvents(GameState state, DayResolutionReport report)
    {
        var keys = new List<string>(state.EventCooldowns.Keys);
        foreach (var key in keys)
        {
            if (state.EventCooldowns[key] > 0)
            {
                state.EventCooldowns[key] -= 1;
            }
        }

        foreach (var evt in EventCatalog.GetAll())
        {
            if (state.GameOver)
            {
                break;
            }

            if (evt is TriggeredEventBase trackedEvent && trackedEvent.IsOnCooldown(state))
            {
                continue;
            }

            if (!evt.ShouldTrigger(state))
            {
                continue;
            }

            evt.Apply(state, report);
            report.AddTriggeredEvent(evt.Name);
        }
    }

    static void ResolveActiveMissions(GameState state, DayResolutionReport report)
    {
        for (var index = state.ActiveMissions.Count - 1; index >= 0; index--)
        {
            var active = state.ActiveMissions[index];
            active.DaysRemaining -= 1;
            if (active.DaysRemaining > 0)
            {
                continue;
            }

            var definition = MissionCatalog.Find(active.MissionId);
            definition?.ResolveOutcome(state, active, report);
            state.ActiveMissions.RemoveAt(index);

            if (definition is not null)
            {
                report.Add(ReasonTags.Mission, $"Mission resolved: {definition.Name}.");
            }
        }
    }

    static void CheckLossConditions(GameState state, DayResolutionReport report)
    {
        if (state.GameOver)
        {
            return;
        }

        var keep = state.GetZone(ZoneId.Keep);
        if (keep.Integrity <= 0)
        {
            state.GameOver = true;
            state.GameOverCause = GameOverCause.KeepBreached;
            state.GameOverDetails = "Keep integrity fell to zero.";
            report.Add(ReasonTags.Event, "Loss: Keep breached.");
            return;
        }

        if (state.Unrest > GameBalance.RevoltThreshold)
        {
            state.GameOver = true;
            state.GameOverCause = GameOverCause.Revolt;
            state.GameOverDetails = $"Unrest exceeded {GameBalance.RevoltThreshold}.";
            report.Add(ReasonTags.Event, "Loss: revolt overwhelmed governance.");
            return;
        }

        if (state.ConsecutiveBothFoodWaterZeroDays >= GameBalance.FoodWaterLossThresholdDays)
        {
            state.GameOver = true;
            state.GameOverCause = GameOverCause.TotalCollapse;
            state.GameOverDetails = "Food and water at zero for 2 consecutive days.";
            report.Add(ReasonTags.Event, "Loss: total collapse from sustained zero food and water.");
            return;
        }

        if (state.Sickness >= 90 && state.Population.HealthyWorkers < 8)
        {
            state.GameOver = true;
            state.GameOverCause = GameOverCause.PandemicCollapse;
            state.GameOverDetails = $"Sickness at {state.Sickness} with only {state.Population.HealthyWorkers} healthy workers. The city cannot function.";
            report.Add(ReasonTags.Event, "Loss: pandemic collapse. Too few healthy workers remain.");
        }
    }

    static void FinalizeDay(GameState state, DayResolutionReport report)
    {
        state.Day += 1;
        state.FoodDeficitYesterday = state.FoodDeficitToday;
        state.WaterDeficitYesterday = state.WaterDeficitToday;

        if (state.Resources[ResourceKind.Food] == 0 && state.Resources[ResourceKind.Water] == 0)
        {
            state.ConsecutiveBothFoodWaterZeroDays += 1;
        }
        else
        {
            state.ConsecutiveBothFoodWaterZeroDays = 0;
        }

        if (!state.GameOver && state.Day >= GameBalance.TargetSurvivalDay)
        {
            state.Survived = true;
            state.GameOver = true;
            state.GameOverCause = GameOverCause.None;
            state.GameOverDetails = "Day 40 reached.";
        }
    }

}
