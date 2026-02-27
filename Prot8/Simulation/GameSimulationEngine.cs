using Prot8.Buildings;
using Prot8.Characters;
using Prot8.Constants;
using Prot8.Diplomacy;
using Prot8.Events;
using Prot8.Laws;
using Prot8.Missions;
using Prot8.Orders;
using Prot8.Resources;
using Prot8.Scavenging;
using Prot8.Trading;
using Prot8.Zones;

namespace Prot8.Simulation;

public sealed class GameSimulationEngine(GameState state)
{
    void RollDailyDisruption()
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
                state.DailyEffects.FoodProductionMultiplier.Apply("Heavy Rains", 0.7);
                state.DailyEffects.WaterProductionMultiplier.Apply("Heavy Rains", 1.3);
                break;
            case 1:
                state.ActiveDisruption = "Cold Snap: Fuel consumption +50% today, +2 sickness.";
                state.DailyEffects.FuelConsumptionMultiplier.Apply("Cold Snap", 1.5);
                break;
            case 2:
                state.ActiveDisruption = "Dust Storm: Materials crafting -40%, siege damage -20% today.";
                state.DailyEffects.MaterialsProductionMultiplier.Apply("Dust Storm", 0.6);
                state.DailyEffects.DustStormActive = true;
                break;
            case 3:
                state.ActiveDisruption = "Clear Skies: All production +15% today.";
                state.DailyEffects.ProductionMultiplier.Apply("Clear Skies", 1.15);
                break;
            case 4:
                state.ActiveDisruption = "Fog Cover: Missions +10% success, repairs -30% today.";
                state.DailyEffects.MissionSuccessBonus = 0.10;
                state.DailyEffects.RepairProductionMultiplier.Apply("Fog Cover", 0.7);
                break;
        }
    }

    public DayResolutionReport StartDay()
    {
        PrepareDay();

        var report = new DayResolutionReport(state.Day)
        {
            StartFood = state.Resources[ResourceKind.Food],
            StartWater = state.Resources[ResourceKind.Water],
            StartFuel = state.Resources[ResourceKind.Fuel],
            StartMorale = state.Morale,
            StartUnrest = state.Unrest,
            StartSickness = state.Sickness,
            StartHealthyWorkers = state.Population.HealthyWorkers,
        };

        // Show last night's scavenging result as dawn entry
        if (GameBalance.EnableNightPhase && state.LastNightResult is { } nightResult)
        {
            var dawnEntry = new ResolutionEntry { Title = "Dawn: Night Scavenging Report" };
            dawnEntry.Write(nightResult.Narrative);
            foreach (var (kind, amount) in nightResult.ResourcesGained)
            {
                dawnEntry.Write($"  +{amount} {kind}");
            }

            if (nightResult.LocationDepleted)
                dawnEntry.Write($"  {nightResult.LocationName} has been picked clean.");
            if (nightResult.IntelGained)
                dawnEntry.Write("  Enemy movements mapped from the watchtower.");

            report.Entries.Add(dawnEntry);
            state.LastNightResult = null;
        }

        RollDailyDisruption();

        // Night phase fatigue penalty from previous night's scavenging
        if (GameBalance.EnableNightPhase && state.FatiguedWorkerCount > 0)
        {
            var total = state.Population.HealthyWorkers;
            if (total > 0)
            {
                var penaltyFraction = (double)state.FatiguedWorkerCount / total;
                var multiplier = 1.0 - penaltyFraction * GameBalance.FatiguedWorkerProductionPenalty;
                if (multiplier < 1.0)
                {
                    state.DailyEffects.ProductionMultiplier.Apply("Scavenging Fatigue", multiplier);
                }
            }

            state.FatiguedWorkerCount = 0;
        }

        if (state.TaintedWellDaysRemaining > 0)
        {
            state.DailyEffects.WaterProductionMultiplier.Apply("Tainted Well", 0.6);
        }

        if (state.IntelBuffDaysRemaining > 0)
        {
            state.DailyEffects.MissionSuccessBonus += GameBalance.IntelMissionSuccessBonus;
        }

        if (GameBalance.EnableNamedCharacters)
        {
            ApplyCharacterTraitBonuses();
        }

        TriggerEvents();

        return report;
    }

    public void ResolveDay(TurnActionChoice action, DayResolutionReport report)
    {
        if (GameBalance.EnableDefensivePosture)
        {
            ApplyDefensivePosture(state, report);
        }

        ApplyPlayerAction(state, action, report);
        ApplyLawPassives(state, report);
        ApplyEmergencyOrderEffects(state, report);

        if (GameBalance.EnableDiplomacy)
        {
            ApplyDiplomacyEffects(state, report);
        }

        var productionEntry = new ResolutionEntry { Title = "Production" };
        var production = CalculateProduction(state, productionEntry);
        if (productionEntry.Messages.Count > 0)
        {
            report.Entries.Add(productionEntry);
        }

        if (GameBalance.EnableKitchenRecipes && state.ActiveKitchenRecipe != KitchenRecipe.Normal)
        {
            var kitchenHasWorkers = state.GetBuilding(BuildingId.FieldKitchen) is { IsDestroyed: false, AssignedWorkers: > 0 };
            if (kitchenHasWorkers)
            {
                var recipeEntry = new ResolutionEntry { Title = "Kitchen Recipe Effects" };
                if (state.ActiveKitchenRecipe == KitchenRecipe.Gruel)
                {
                    state.AddSickness(GameBalance.GruelSicknessPerDay, recipeEntry);
                    recipeEntry.Write("Gruel rations are taking a toll on health.");
                }
                else if (state.ActiveKitchenRecipe == KitchenRecipe.Feast)
                {
                    state.AddMorale(GameBalance.FeastMoralePerDay, recipeEntry);
                    recipeEntry.Write("The feast lifts spirits across the city.");
                }
                report.Entries.Add(recipeEntry);
            }
        }

        if (GameBalance.EnableBuildingSpecializations)
        {
            ApplyBuildingSpecializationPassives(state, report);
        }

        TradingEngine.ResolveTrades(state, report);

        var consumptionEntry = new ResolutionEntry { Title = "Consumption" };
        ApplyConsumption(state, report, consumptionEntry);
        if (consumptionEntry.Messages.Count > 0)
        {
            report.Entries.Add(consumptionEntry);
        }

        var deficitEntry = new ResolutionEntry { Title = "Deficit Pressure" };
        ApplyDeficitPenalties(state, deficitEntry);
        if (deficitEntry.Messages.Count > 0)
        {
            report.Entries.Add(deficitEntry);
        }

        var overcrowdingEntry = new ResolutionEntry { Title = "Overcrowding" };
        ApplyOvercrowdingPenalties(state, report, overcrowdingEntry);
        if (overcrowdingEntry.Messages.Count > 0)
        {
            report.Entries.Add(overcrowdingEntry);
        }

        var sicknessEntry = new ResolutionEntry { Title = "Sickness & Recovery" };
        ApplySicknessProgression(state, production, report, sicknessEntry);
        if (sicknessEntry.Messages.Count > 0)
        {
            report.Entries.Add(sicknessEntry);
        }

        var unrestEntry = new ResolutionEntry { Title = "Morale & Unrest" };
        ApplyUnrestProgression(state, unrestEntry);
        if (unrestEntry.Messages.Count > 0)
        {
            report.Entries.Add(unrestEntry);
        }

        var siegeEntry = new ResolutionEntry { Title = "Siege" };
        ApplySiegeDamage(state, siegeEntry);
        if (siegeEntry.Messages.Count > 0)
        {
            report.Entries.Add(siegeEntry);
        }

        var repairEntry = new ResolutionEntry { Title = "Repairs" };
        ApplyRepairs(state, production, repairEntry);
        if (repairEntry.Messages.Count > 0)
        {
            report.Entries.Add(repairEntry);
        }

        if (!GameBalance.EnableNightPhase)
        {
            ResolveActiveMissions(state, report);
        }

        var statusEntry = new ResolutionEntry { Title = "Status" };
        CheckLossConditions(state, statusEntry);
        if (statusEntry.Messages.Count > 0)
        {
            report.Entries.Add(statusEntry);
        }

        FinalizeDay(state);
    }

    void PrepareDay()
    {
        state.ActiveOrderId = null;
        state.FoodDeficitToday = false;
        state.WaterDeficitToday = false;
        state.FuelDeficitToday = false;
        state.DeathsAtStartOfDay = state.TotalDeaths;

        state.Flags.TickDay();

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
            state.TaintedWellDaysRemaining -= 1;
        }

        if (state.IntelBuffDaysRemaining > 0)
        {
            state.IntelBuffDaysRemaining -= 1;
        }

        if (GameBalance.EnableBuildingUpgrades)
        {
            foreach (var building in state.GetActiveBuildings())
            {
                if (building.UpgradeDaysRemaining > 0)
                {
                    building.UpgradeDaysRemaining -= 1;
                    if (building.UpgradeDaysRemaining <= 0)
                    {
                        building.UpgradeLevel++;
                    }
                }
            }
        }
    }

    static void ApplyDefensivePosture(GameState state, DayResolutionReport report)
    {
        if (state.CurrentPosture == DefensivePosture.None)
            return;

        var entry = new ResolutionEntry { Title = "Defensive Posture" };

        switch (state.CurrentPosture)
        {
            case DefensivePosture.OpenGates:
                state.AddMorale(GameBalance.OpenGatesMorale, entry);
                if (state.RollPercent() <= GameBalance.OpenGatesRefugeeChance)
                {
                    var refugees = state.Random.Next(GameBalance.OpenGatesRefugeeMin, GameBalance.OpenGatesRefugeeMax);
                    state.Population.HealthyWorkers += refugees;
                    entry.Write($"{refugees} refugees arrive through the open gates.");
                }
                if (state.RollPercent() <= GameBalance.OpenGatesInfiltratorChance)
                {
                    state.AddUnrest(GameBalance.OpenGatesInfiltratorUnrest, entry);
                    state.AddSickness(GameBalance.OpenGatesInfiltratorSickness, entry);
                    entry.Write("Infiltrators slipped in among the refugees, sowing chaos and disease.");
                }
                break;

            case DefensivePosture.AggressivePatrols:
                state.AddUnrest(-GameBalance.AggressivePatrolsUnrest, entry);
                if (state.RollPercent() <= GameBalance.AggressivePatrolsInterceptChance)
                {
                    var resourceKinds = new[] { ResourceKind.Food, ResourceKind.Water, ResourceKind.Fuel, ResourceKind.Materials, ResourceKind.Medicine };
                    var kind = resourceKinds[state.Random.Next(0, resourceKinds.Length)];
                    var amount = state.Random.Next(GameBalance.AggressivePatrolsResourceMin, GameBalance.AggressivePatrolsResourceMax);
                    state.AddResource(kind, amount, entry);
                    entry.Write($"Patrol intercepted enemy supplies: +{amount} {kind}.");
                }
                entry.Write("Guard patrols maintain order in the streets.");
                break;

            case DefensivePosture.ScorchedPerimeter:
                var perimeter = state.ActivePerimeterZone;
                if (!state.ScorchedPerimeterUsed.ContainsKey(perimeter.Id))
                {
                    state.ScorchedPerimeterUsed[perimeter.Id] = true;
                    perimeter.Integrity = Math.Max(0, perimeter.Integrity - GameBalance.ScorchedPerimeterIntegrityDamage);
                    state.ScorchedPerimeterDamageReductionDays = GameBalance.ScorchedPerimeterDuration;
                    state.AddMorale(GameBalance.ScorchedPerimeterMorale, entry);
                    state.Flags.Tyranny.Add(GameBalance.ScorchedPerimeterTyranny);
                    entry.Write($"The perimeter of {perimeter.Name} is set ablaze. -{GameBalance.ScorchedPerimeterIntegrityDamage} integrity, but siege damage reduced for {GameBalance.ScorchedPerimeterDuration} days.");
                }
                state.CurrentPosture = DefensivePosture.None;
                break;
        }

        if (entry.Messages.Count > 0)
            report.Entries.Add(entry);
    }

    static void ApplyPlayerAction(GameState state, TurnActionChoice action, DayResolutionReport report)
    {
        if (!string.IsNullOrWhiteSpace(action.LawId))
        {
            var law = LawCatalog.Find(action.LawId);
            if (law is null)
            {
                var failEntry = new ResolutionEntry { Title = "Law Action" };
                failEntry.Write("Law selection failed: law not found.");
                report.Entries.Add(failEntry);
                return;
            }

            if (state.ActiveLawIds.Contains(law.Id))
            {
                var failEntry = new ResolutionEntry { Title = $"Law: {law.Name}" };
                failEntry.Write($"Law already enacted: {law.Name}.");
                report.Entries.Add(failEntry);
                return;
            }

            var lawCooldownActive = state.LastLawDay != int.MinValue
                                    && state.Day - state.LastLawDay < GameBalance.LawCooldownDays;
            if (lawCooldownActive)
            {
                var nextDay = state.LastLawDay + GameBalance.LawCooldownDays;
                var failEntry = new ResolutionEntry { Title = $"Law: {law.Name}" };
                failEntry.Write($"Law cooldown active. Next enactment day: {nextDay}.");
                report.Entries.Add(failEntry);
                return;
            }

            if (!law.CanEnact(state))
            {
                var failEntry = new ResolutionEntry { Title = $"Law: {law.Name}" };
                failEntry.Write($"Cannot enact {law.Name}.");
                report.Entries.Add(failEntry);
                return;
            }

            state.ActiveLawIds.Add(law.Id);
            state.LastLawDay = state.Day;
            if (!state.FirstLawDay.HasValue)
            {
                state.FirstLawDay = state.Day;
                state.FirstLawName = law.Name;
            }

            var lawEntry = new ResolutionEntry { Title = $"Law Enacted: {law.Name}" };
            law.OnEnact(state, lawEntry);
            report.Entries.Add(lawEntry);
            return;
        }

        if (!string.IsNullOrWhiteSpace(action.EmergencyOrderId))
        {
            var order = EmergencyOrderCatalog.Find(action.EmergencyOrderId);
            if (order is null)
            {
                var failEntry = new ResolutionEntry { Title = "Order Action" };
                failEntry.Write("Emergency order selection failed: order not found.");
                report.Entries.Add(failEntry);
                return;
            }

            if (state.OrderCooldowns.TryGetValue(order.Id, out var lastDay)
                && state.Day - lastDay < order.CooldownDays)
            {
                var nextDay = lastDay + order.CooldownDays;
                var failEntry = new ResolutionEntry { Title = $"Order: {order.Name}" };
                failEntry.Write($"Emergency order cooldown active for {order.Name}. Next available day: {nextDay}.");
                report.Entries.Add(failEntry);
                return;
            }

            if (!order.CanIssue(state))
            {
                var failEntry = new ResolutionEntry { Title = $"Order: {order.Name}" };
                failEntry.Write($"Cannot issue {order.Name}.");
                report.Entries.Add(failEntry);
                return;
            }

            state.ActiveOrderId = order.Id;
            state.OrderCooldowns[order.Id] = state.Day;
            var orderEntry = new ResolutionEntry { Title = $"Order Prepared: {order.Name}" };
            orderEntry.Write($"Emergency order prepared: {order.Name}.");
            report.Entries.Add(orderEntry);
            return;
        }

        if (!string.IsNullOrWhiteSpace(action.MissionId))
        {
            if (GameBalance.EnableDefensivePosture && state.CurrentPosture == DefensivePosture.HunkerDown)
            {
                var failEntry = new ResolutionEntry { Title = "Mission Action" };
                failEntry.Write("Cannot start missions while in Hunker Down posture.");
                report.Entries.Add(failEntry);
                return;
            }

            var mission = MissionCatalog.Find(action.MissionId);
            if (mission is null)
            {
                var failEntry = new ResolutionEntry { Title = "Mission Action" };
                failEntry.Write("Mission selection failed: mission not found.");
                report.Entries.Add(failEntry);
                return;
            }

            if (state.MissionCooldowns.TryGetValue(mission.Id, out var lastMissionDay))
            {
                if (state.Day - lastMissionDay < GameBalance.MissionCooldownDays)
                {
                    var nextDay = lastMissionDay + GameBalance.MissionCooldownDays;
                    var failEntry = new ResolutionEntry { Title = $"Mission: {mission.Name}" };
                    failEntry.Write($"Mission cooldown active for {mission.Name}. Next available day: {nextDay}.");
                    report.Entries.Add(failEntry);
                    return;
                }
            }

            if (!mission.CanStart(state))
            {
                var failEntry = new ResolutionEntry { Title = $"Mission: {mission.Name}" };
                failEntry.Write($"Cannot start mission {mission.Name}.");
                report.Entries.Add(failEntry);
                return;
            }

            if (state.IdleWorkers < mission.WorkerCost)
            {
                var failEntry = new ResolutionEntry { Title = $"Mission: {mission.Name}" };
                failEntry.Write(
                    $"Cannot start mission {mission.Name}: not enough idle workers (need {mission.WorkerCost}, have {state.IdleWorkers}).");
                report.Entries.Add(failEntry);
                return;
            }

            if (mission.GuardCost > 0)
            {
                var availableGuards = state.Population.Guards - state.ReservedGuardsForMissions;
                if (availableGuards < mission.GuardCost)
                {
                    var failEntry = new ResolutionEntry { Title = $"Mission: {mission.Name}" };
                    failEntry.Write(
                        $"Cannot start mission {mission.Name}: not enough available guards (need {mission.GuardCost}, have {availableGuards}).");
                    report.Entries.Add(failEntry);
                    return;
                }
            }

            state.ActiveMissions.Add(new ActiveMission(mission));
            state.MissionCooldowns[mission.Id] = state.Day;
            var missionEntry = new ResolutionEntry { Title = $"Mission Started: {mission.Name}" };
            missionEntry.Write(
                $"Mission started: {mission.Name} ({mission.DurationDays} day(s), {mission.WorkerCost} workers committed).");
            report.Entries.Add(missionEntry);
        }
    }

    static void ApplyLawPassives(GameState state, DayResolutionReport report)
    {
        foreach (var lawId in state.ActiveLawIds)
        {
            var law = LawCatalog.Find(lawId);
            if (law is null)
            {
                continue;
            }

            if (GameBalance.EnableDefensivePosture && state.AreGuardsCommitted
                && (law.Id == "mandatory_guard_service" || law.Id == "garrison_mandate"))
            {
                if (!GameBalance.EnableDefensivePostureGuardOverride)
                {
                    var skipEntry = new ResolutionEntry { Title = $"Law: {law.Name}" };
                    skipEntry.Write($"{law.Name} daily effects suppressed — guards committed to defensive posture.");
                    report.Entries.Add(skipEntry);
                }
                continue;
            }

            var lawEntry = new ResolutionEntry { Title = $"Law: {law.Name}" };
            law.ApplyDaily(state, lawEntry);
            if (lawEntry.Messages.Count > 0)
            {
                report.Entries.Add(lawEntry);
            }
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

        var orderEntry = new ResolutionEntry { Title = $"Order: {order.Name}" };
        order.Apply(state, orderEntry);
        if (orderEntry.Messages.Count > 0)
        {
            report.Entries.Add(orderEntry);
        }
    }

    static void ApplyDiplomacyEffects(GameState state, DayResolutionReport report)
    {
        foreach (var actionId in state.ActiveDiplomacyIds)
        {
            var action = DiplomacyCatalog.Find(actionId);
            if (action is null)
                continue;

            var entry = new ResolutionEntry { Title = $"Diplomacy: {action.Name}" };
            action.ApplyDaily(state, entry);
            if (entry.Messages.Count > 0)
                report.Entries.Add(entry);
        }
    }

    static DailyProductionResult CalculateProduction(GameState state, ResolutionEntry entry)
    {
        var result = new DailyProductionResult();

        var statMult = StatModifiers.ComputeGlobalProductionMultiplier(state);
        var globalMultiplier = Math.Clamp(statMult.Value, 0.25, 1.3) *
                               state.DailyEffects.ProductionMultiplier.Value;

        foreach (var building in state.GetActiveBuildings())
        {
            var workers = building.AssignedWorkers;
            if (workers <= 0)
            {
                continue;
            }

            // Kitchen recipe override for Field Kitchen
            if (GameBalance.EnableKitchenRecipes && building.Id == BuildingId.FieldKitchen &&
                state.ActiveKitchenRecipe != KitchenRecipe.Normal)
            {
                ApplyKitchenRecipeProduction(state, building, workers, globalMultiplier, result, entry);
                continue;
            }

            // Building specialization override
            if (GameBalance.EnableBuildingSpecializations)
            {
                var spec = state.GetBuildingSpec(building.Id);
                if (spec != BuildingSpecialization.None
                    && ApplySpecializedProduction(state, building, workers, globalMultiplier, spec, result, entry))
                {
                    continue;
                }
            }

            var resourceMultiplier = 1.0;

            if (state.DailyEffects.QuarantineZone.HasValue && state.DailyEffects.QuarantineZone.Value == building.Zone)
            {
                resourceMultiplier *= 0.5;
            }

            resourceMultiplier *= GetResourceProductionMultiplier(state, building);

            if (GameBalance.EnableBuildingUpgrades && building.UpgradeLevel > 0)
            {
                resourceMultiplier *= 1 + building.UpgradeLevel * GameBalance.BuildingUpgradeBonusPerLevel;
            }

            var nominalCycles = workers * globalMultiplier * resourceMultiplier;
            if (nominalCycles <= 0)
            {
                continue;
            }

            var scale = 1.0;
            var inputs = building.Inputs;
            var hasInput = inputs.Count > 0;
            if (hasInput)
            {
                foreach (var pair in inputs)
                {
                    var perCycle = pair.Quantity;
                    if (building.Id == BuildingId.Clinic && pair.Resource == ResourceKind.Medicine)
                    {
                        perCycle *= state.DailyEffects.MedicineUsageMultiplier.Value;
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
                    if (building.Id == BuildingId.Clinic && pair.Resource == ResourceKind.Medicine)
                    {
                        perCycle *= state.DailyEffects.MedicineUsageMultiplier.Value;
                    }

                    var spend = Math.Min((int)Math.Ceiling(effectiveCycles * perCycle), state.Resources[pair.Resource]);
                    if (spend > 0)
                    {
                        state.Resources.Consume(pair.Resource, spend);
                        entry.Write($"{building.Name}: consumed {spend} {pair.Resource}.");
                        if (building.Id == BuildingId.Clinic && pair.Resource == ResourceKind.Medicine)
                        {
                            result.ClinicMedicineSpent += spend;
                        }
                    }
                }
            }

            var outputResource = building.Outputs.FirstOrDefault();
            var produced = (int)Math.Floor(effectiveCycles * outputResource.Quantity);
            if (produced <= 0)
            {
                continue;
            }

            if (outputResource.Resource is not ResourceKind.Integrity and not ResourceKind.Care)
            {
                if (!GameBalance.WasteExcessResources)
                {
                    var space = state.Resources.GetAvailableSpace(outputResource.Resource);
                    if (produced > space)
                        produced = space;
                    if (produced <= 0) continue;
                }

                var actualAdded = state.Resources.Add(outputResource.Resource, produced);
                result.AddResourceProduction(outputResource.Resource, actualAdded);
                if (actualAdded < produced)
                {
                    var wasted = produced - actualAdded;
                    entry.Write($"{building.Name}: +{actualAdded} {outputResource.Resource} ({wasted} wasted — storage full).");
                }
                else
                {
                    entry.Write($"{building.Name}: +{produced} {outputResource.Resource}.");
                }
            }
            else if (outputResource.Resource == ResourceKind.Integrity)
            {
                result.RepairPoints += produced;
                entry.Write($"{building.Name} prepared {produced} integrity points.");
            }
            else if (building.Id == BuildingId.Clinic)
            {
                result.ClinicCarePoints += produced;
                result.ClinicSlotsUsed += (int)Math.Max(1, Math.Floor(effectiveCycles));
                entry.Write($"Clinic care capacity: {produced} care points.");
            }
        }

        return result;
    }

    static double GetResourceProductionMultiplier(GameState state, BuildingState building)
    {
        var outputResource = building.Outputs.FirstOrDefault().Resource;
        return outputResource switch
        {
            ResourceKind.Food => state.DailyEffects.FoodProductionMultiplier.Value,
            ResourceKind.Water => state.DailyEffects.WaterProductionMultiplier.Value,
            ResourceKind.Materials => state.DailyEffects.MaterialsProductionMultiplier.Value,
            ResourceKind.Integrity => state.DailyEffects.RepairProductionMultiplier.Value,
            _ => 1.0,
        };
    }

    static void ApplyKitchenRecipeProduction(GameState state, BuildingState building, int workers,
        double globalMultiplier, DailyProductionResult result, ResolutionEntry entry)
    {
        var recipe = state.ActiveKitchenRecipe;

        if (recipe == KitchenRecipe.Gruel)
        {
            // Gruel: no fuel input, output = workers * globalMult * GruelFoodPerWorker
            var produced = (int)Math.Floor(workers * globalMultiplier * GameBalance.GruelFoodPerWorker);
            if (produced > 0)
            {
                var actualAdded = state.Resources.Add(ResourceKind.Food, produced);
                result.AddResourceProduction(ResourceKind.Food, actualAdded);
                entry.Write($"{building.Name} (Gruel): +{actualAdded} Food (no fuel used).");
            }
        }
        else if (recipe == KitchenRecipe.Feast)
        {
            // Feast: fuel input = FeastFuelPerWorker per worker, output = workers * globalMult * FeastFoodPerWorker
            var fuelNeeded = (int)Math.Ceiling(workers * GameBalance.FeastFuelPerWorker);
            var fuelAvailable = state.Resources[ResourceKind.Fuel];
            var scale = fuelNeeded > fuelAvailable && fuelNeeded > 0
                ? (double)fuelAvailable / fuelNeeded
                : 1.0;

            var fuelSpend = (int)Math.Ceiling(fuelNeeded * scale);
            if (fuelSpend > 0)
            {
                state.Resources.Consume(ResourceKind.Fuel, fuelSpend);
                entry.Write($"{building.Name} (Feast): consumed {fuelSpend} Fuel.");
            }

            var produced = (int)Math.Floor(workers * globalMultiplier * scale * GameBalance.FeastFoodPerWorker);
            if (produced > 0)
            {
                var actualAdded = state.Resources.Add(ResourceKind.Food, produced);
                result.AddResourceProduction(ResourceKind.Food, actualAdded);
                entry.Write($"{building.Name} (Feast): +{actualAdded} Food.");
            }
        }
    }

    static bool ApplySpecializedProduction(GameState state, BuildingState building, int workers,
        double globalMultiplier, BuildingSpecialization spec, DailyProductionResult result, ResolutionEntry entry)
    {
        switch (spec)
        {
            case BuildingSpecialization.GrainSilos:
                ProduceSimple(state, building, workers, globalMultiplier, ResourceKind.Food, GameBalance.GrainSilosFoodPerWorker, result, entry);
                return true;

            case BuildingSpecialization.MedicinalHerbs:
                ProduceSimple(state, building, workers, globalMultiplier, ResourceKind.Food, GameBalance.MedicinalHerbsFoodPerWorker, result, entry);
                var medProduced = (int)Math.Floor(workers * globalMultiplier * GameBalance.MedicinalHerbsMedicinePerWorker);
                if (medProduced > 0)
                {
                    var added = state.Resources.Add(ResourceKind.Medicine, medProduced);
                    result.AddResourceProduction(ResourceKind.Medicine, added);
                    entry.Write($"{building.Name}: +{added} Medicine.");
                }
                return true;

            case BuildingSpecialization.ApothecaryLab:
                ProduceWithInput(state, building, workers, globalMultiplier, ResourceKind.Fuel, GameBalance.ApothecaryLabFuelInput,
                    ResourceKind.Medicine, GameBalance.ApothecaryLabMedicinePerWorker, result, entry);
                return true;

            case BuildingSpecialization.DeepBoring:
                ProduceWithInput(state, building, workers, globalMultiplier, ResourceKind.Fuel, GameBalance.DeepBoringFuelInput,
                    ResourceKind.Water, GameBalance.DeepBoringWaterPerWorker, result, entry);
                return true;

            case BuildingSpecialization.CoalPits:
                ProduceSimple(state, building, workers, globalMultiplier, ResourceKind.Fuel, GameBalance.CoalPitsFuelPerWorker, result, entry);
                return true;

            case BuildingSpecialization.RationedDistribution:
                ProduceSimple(state, building, workers, globalMultiplier, ResourceKind.Fuel, GameBalance.RationedDistributionFuelPerWorker, result, entry);
                return true;

            case BuildingSpecialization.SoupLine:
                ProduceWithInput(state, building, workers, globalMultiplier, ResourceKind.Fuel, 1,
                    ResourceKind.Food, GameBalance.SoupLineFoodPerWorker, result, entry);
                return true;

            case BuildingSpecialization.ArmsFoundry:
                ProduceWithInput(state, building, workers, globalMultiplier, ResourceKind.Fuel, GameBalance.ArmsFoundryFuelInput,
                    ResourceKind.Materials, GameBalance.ArmsFoundryMaterialsPerWorker, result, entry);
                return true;

            case BuildingSpecialization.WarSmith:
                ProduceIntegrityWithInput(state, building, workers, globalMultiplier, ResourceKind.Materials, GameBalance.WarSmithMaterialsInput,
                    GameBalance.WarSmithIntegrityPerWorker, result, entry);
                return true;

            case BuildingSpecialization.SiegeWorkshop:
                ProduceIntegrityWithInput(state, building, workers, globalMultiplier, ResourceKind.Materials, GameBalance.SiegeWorkshopMaterialsInput,
                    GameBalance.SiegeWorkshopIntegrityPerWorker, result, entry);
                return true;

            case BuildingSpecialization.RainCollection:
                var rainMult = state.DailyEffects.FoodProductionMultiplier.Entries.Any(e => e.Source == "Heavy Rains")
                    ? GameBalance.RainCollectionHeavyRainsMultiplier : 1.0;
                ProduceSimple(state, building, workers, globalMultiplier * rainMult, ResourceKind.Water,
                    GameBalance.RainCollectionWaterPerWorker, result, entry);
                return true;

            case BuildingSpecialization.PreservedStores:
                ProduceSimple(state, building, workers, globalMultiplier, ResourceKind.Food, GameBalance.PreservedStoresFoodPerWorker, result, entry);
                return true;

            case BuildingSpecialization.MushroomFarm:
                ProduceSimple(state, building, workers, globalMultiplier, ResourceKind.Food, GameBalance.MushroomFarmFoodPerWorker, result, entry);
                return true;

            case BuildingSpecialization.DistributionHub:
                ProduceSimple(state, building, workers, globalMultiplier, ResourceKind.Water, GameBalance.DistributionHubWaterPerWorker, result, entry);
                return true;

            case BuildingSpecialization.PropagandaPost:
                // No resource output — passive effects handled in ApplyUnrestProgression/morale
                return true;

            case BuildingSpecialization.WeaponCache:
                // No fuel output — passive effects handled in ApplyUnrestProgression
                return true;

            default:
                // Specs like HealersRefuge, PurificationBasin, FortifiedKitchen, SalvageYard,
                // ArmorWorks, EmergencyReserve, EmergencySupplies, EngineerCorps — use default production
                return false;
        }
    }

    static void ProduceSimple(GameState state, BuildingState building, int workers, double globalMultiplier,
        ResourceKind output, double outputPerWorker, DailyProductionResult result, ResolutionEntry entry)
    {
        var produced = (int)Math.Floor(workers * globalMultiplier * outputPerWorker);
        if (produced <= 0) return;
        var added = state.Resources.Add(output, produced);
        result.AddResourceProduction(output, added);
        entry.Write($"{building.Name}: +{added} {output}.");
    }

    static void ProduceWithInput(GameState state, BuildingState building, int workers, double globalMultiplier,
        ResourceKind inputKind, double inputPerWorker, ResourceKind outputKind, double outputPerWorker,
        DailyProductionResult result, ResolutionEntry entry)
    {
        var nominalCycles = workers * globalMultiplier;
        var inputNeeded = nominalCycles * inputPerWorker;
        var available = state.Resources[inputKind];
        var scale = inputNeeded > available && inputNeeded > 0 ? available / inputNeeded : 1.0;
        var effectiveCycles = nominalCycles * scale;
        if (effectiveCycles <= 0) return;

        var inputSpend = Math.Min((int)Math.Ceiling(effectiveCycles * inputPerWorker), state.Resources[inputKind]);
        if (inputSpend > 0)
        {
            state.Resources.Consume(inputKind, inputSpend);
            entry.Write($"{building.Name}: consumed {inputSpend} {inputKind}.");
        }

        var produced = (int)Math.Floor(effectiveCycles * outputPerWorker);
        if (produced <= 0) return;
        var added = state.Resources.Add(outputKind, produced);
        result.AddResourceProduction(outputKind, added);
        entry.Write($"{building.Name}: +{added} {outputKind}.");
    }

    static void ProduceIntegrityWithInput(GameState state, BuildingState building, int workers, double globalMultiplier,
        ResourceKind inputKind, double inputPerWorker, double integrityPerWorker,
        DailyProductionResult result, ResolutionEntry entry)
    {
        var nominalCycles = workers * globalMultiplier;
        var inputNeeded = nominalCycles * inputPerWorker;
        var available = state.Resources[inputKind];
        var scale = inputNeeded > available && inputNeeded > 0 ? available / inputNeeded : 1.0;
        var effectiveCycles = nominalCycles * scale;
        if (effectiveCycles <= 0) return;

        var inputSpend = Math.Min((int)Math.Ceiling(effectiveCycles * inputPerWorker), state.Resources[inputKind]);
        if (inputSpend > 0)
        {
            state.Resources.Consume(inputKind, inputSpend);
            entry.Write($"{building.Name}: consumed {inputSpend} {inputKind}.");
        }

        var produced = (int)Math.Floor(effectiveCycles * integrityPerWorker);
        if (produced > 0)
        {
            result.RepairPoints += produced;
            entry.Write($"{building.Name} prepared {produced} integrity points.");
        }
    }

    static void ApplyBuildingSpecializationPassives(GameState state, DayResolutionReport report)
    {
        // SalvageYard daily random resource
        if (state.GetBuildingSpec(BuildingId.Workshop) == BuildingSpecialization.SalvageYard
            && !state.GetBuilding(BuildingId.Workshop).IsDestroyed
            && state.GetBuilding(BuildingId.Workshop).AssignedWorkers > 0)
        {
            if (state.RollPercent() <= GameBalance.SalvageYardChance)
            {
                var specEntry = new ResolutionEntry { Title = "Salvage Yard" };
                var resourceKinds = new[] { ResourceKind.Food, ResourceKind.Water, ResourceKind.Fuel, ResourceKind.Materials, ResourceKind.Medicine };
                var kind = resourceKinds[state.Random.Next(0, resourceKinds.Length)];
                state.AddResource(kind, GameBalance.SalvageYardAmount, specEntry);
                specEntry.Write($"Salvage crews recovered {GameBalance.SalvageYardAmount} {kind} from the ruins.");
                report.Entries.Add(specEntry);
            }
        }

        // EmergencyReserve water-zero auto-release
        if (state.GetBuildingSpec(BuildingId.Cistern) == BuildingSpecialization.EmergencyReserve
            && !state.GetBuilding(BuildingId.Cistern).IsDestroyed
            && !state.EmergencyWaterReserveUsed
            && state.Resources[ResourceKind.Water] == 0)
        {
            var specEntry = new ResolutionEntry { Title = "Emergency Reserve" };
            state.Resources.Add(ResourceKind.Water, 10);
            state.EmergencyWaterReserveUsed = true;
            specEntry.Write("Emergency water reserves released! +10 water.");
            report.Entries.Add(specEntry);
        }
    }

    static void ApplyConsumption(GameState state, DayResolutionReport report, ResolutionEntry entry)
    {
        if (GameBalance.EnableBuildingSpecializations)
        {
            if (state.GetBuildingSpec(BuildingId.FuelStore) == BuildingSpecialization.RationedDistribution
                && !state.GetBuilding(BuildingId.FuelStore).IsDestroyed)
                state.DailyEffects.FuelConsumptionMultiplier.Apply("Rationed Distribution", GameBalance.RationedDistributionFuelConsumptionMultiplier);

            if (state.GetBuildingSpec(BuildingId.RootCellar) == BuildingSpecialization.PreservedStores
                && !state.GetBuilding(BuildingId.RootCellar).IsDestroyed)
                state.DailyEffects.FoodConsumptionMultiplier.Apply("Preserved Stores", GameBalance.PreservedStoresFoodConsumptionMultiplier);

            if (state.GetBuildingSpec(BuildingId.RationingPost) == BuildingSpecialization.DistributionHub
                && !state.GetBuilding(BuildingId.RationingPost).IsDestroyed)
                state.DailyEffects.FoodConsumptionMultiplier.Apply("Distribution Hub", GameBalance.DistributionHubFoodConsumptionMultiplier);
        }

        var population = state.Population.TotalPopulation;

        var foodNeed = (int)Math.Ceiling(population * GameBalance.FoodPerPersonPerDay *
                                         state.DailyEffects.FoodConsumptionMultiplier.Value);
        var waterNeed = (int)Math.Ceiling(population * GameBalance.WaterPerPersonPerDay *
                                          state.DailyEffects.WaterConsumptionMultiplier.Value);
        var fuelNeed = (int)Math.Ceiling(population * GameBalance.FuelPerPersonPerDay *
                                         state.DailyEffects.FuelConsumptionMultiplier.Value);

        var foodConsumed = state.Resources.Consume(ResourceKind.Food, foodNeed);
        var waterConsumed = state.Resources.Consume(ResourceKind.Water, waterNeed);
        var fuelConsumed = state.Resources.Consume(ResourceKind.Fuel, fuelNeed);

        report.FoodConsumedToday = foodConsumed;
        report.WaterConsumedToday = waterConsumed;
        state.LastDayFoodConsumed = foodConsumed;
        state.LastDayWaterConsumed = waterConsumed;

        entry.Write(
            $"Daily consumption: food {foodConsumed}/{foodNeed}, water {waterConsumed}/{waterNeed}, fuel {fuelConsumed}/{fuelNeed}.");

        if (foodConsumed < foodNeed)
        {
            state.FoodDeficitToday = true;
            report.FoodDeficitToday = true;
            entry.Write($"Food deficit: short by {foodNeed - foodConsumed}.");
        }

        if (waterConsumed < waterNeed)
        {
            state.WaterDeficitToday = true;
            report.WaterDeficitToday = true;
            entry.Write($"Water deficit: short by {waterNeed - waterConsumed}.");
        }

        if (fuelConsumed < fuelNeed)
        {
            state.FuelDeficitToday = true;
            report.FuelDeficitToday = true;
            entry.Write($"Fuel deficit: short by {fuelNeed - fuelConsumed}.");
        }
    }

    static void ApplyDeficitPenalties(GameState state, ResolutionEntry entry)
    {
        if (state.FoodDeficitToday)
        {
            state.DayFirstFoodDeficit ??= state.Day;

            state.ConsecutiveFoodDeficitDays += 1;
            state.AddUnrest(6, entry);
            state.AddMorale(-8, entry);
            state.AddSickness(2, entry);
        }
        else
        {
            state.ConsecutiveFoodDeficitDays = 0;
        }

        if (state.WaterDeficitToday)
        {
            state.DayFirstWaterDeficit ??= state.Day;

            state.ConsecutiveWaterDeficitDays += 1;
            state.AddUnrest(7, entry);
            state.AddMorale(-10, entry);
            state.AddSickness(6, entry);
        }
        else
        {
            state.ConsecutiveWaterDeficitDays = 0;
        }

        if (state.FuelDeficitToday)
        {
            state.AddMorale(-4, entry);
            state.AddSickness(2, entry);
        }
    }

    static void ApplyOvercrowdingPenalties(GameState state, DayResolutionReport report, ResolutionEntry entry)
    {
        var totalPop = state.Population.TotalPopulation;
        var totalCapacity = 0;
        foreach (var zone in state.Zones)
        {
            if (!zone.IsLost)
            {
                totalCapacity += zone.Capacity;
            }
        }

        var overflow = totalPop - totalCapacity;
        var totalStacks = overflow > 0 ? overflow / GameBalance.OvercrowdingThreshold : 0;

        if (totalStacks <= 0)
        {
            report.OvercrowdingStacksToday = 0;
            return;
        }

        report.OvercrowdingStacksToday = totalStacks;

        state.AddUnrest(totalStacks * GameBalance.OvercrowdingUnrestPerStack, entry);
        state.AddSickness(totalStacks * GameBalance.OvercrowdingSicknessPerStack, entry);

        var multiplier = totalStacks * GameBalance.OvercrowdingConsumptionPerStack;
        var extraFood = (int)Math.Ceiling(report.FoodConsumedToday * multiplier);
        var extraWater = (int)Math.Ceiling(report.WaterConsumedToday * multiplier);

        if (extraFood > 0)
        {
            var consumed = state.Resources.Consume(ResourceKind.Food, extraFood);
            entry.Write($"Overcrowding strain: additional food consumption {consumed}/{extraFood}.");
        }

        if (extraWater > 0)
        {
            var consumed = state.Resources.Consume(ResourceKind.Water, extraWater);
            entry.Write($"Overcrowding strain: additional water consumption {consumed}/{extraWater}.");
        }
    }

    static void ApplySicknessProgression(GameState state, DailyProductionResult production, DayResolutionReport report,
        ResolutionEntry entry)
    {
        var sicknessDeltaTracked = StatModifiers.ComputeSicknessFromEnvironment(state);
        var sicknessDelta = sicknessDeltaTracked.Value;
        sicknessDelta -= state.DailyEffects.QuarantineSicknessReduction;
        sicknessDelta -= production.ClinicCarePoints / 3;

        if (GameBalance.EnableClinicSpecialization && state.ClinicSpecialization == ClinicSpecialization.QuarantineWard)
        {
            sicknessDelta -= GameBalance.QuarantineWardSicknessReduction;
        }

        if (GameBalance.EnableBuildingSpecializations)
        {
            if (state.GetBuildingSpec(BuildingId.HerbGarden) == BuildingSpecialization.HealersRefuge
                && !state.GetBuilding(BuildingId.HerbGarden).IsDestroyed)
                sicknessDelta -= GameBalance.HealersRefugeSicknessReduction;

            if (state.GetBuildingSpec(BuildingId.Well) == BuildingSpecialization.PurificationBasin
                && !state.GetBuilding(BuildingId.Well).IsDestroyed)
                sicknessDelta -= GameBalance.PurificationBasinSicknessReduction;

            if (state.GetBuildingSpec(BuildingId.FuelStore) == BuildingSpecialization.CoalPits
                && !state.GetBuilding(BuildingId.FuelStore).IsDestroyed
                && state.GetBuilding(BuildingId.FuelStore).AssignedWorkers > 0)
                sicknessDelta += GameBalance.CoalPitsDailySickness;

            if (state.GetBuildingSpec(BuildingId.RootCellar) == BuildingSpecialization.MushroomFarm
                && !state.GetBuilding(BuildingId.RootCellar).IsDestroyed
                && state.GetBuilding(BuildingId.RootCellar).AssignedWorkers > 0)
                sicknessDelta += GameBalance.MushroomFarmDailySickness;
        }

        if (state.DailyEffects.FuelConsumptionMultiplier.Value > 1.0)
        {
            sicknessDelta += 2;
        }

        if (sicknessDelta != 0)
        {
            state.AddSickness(sicknessDelta, entry);
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
            state.ConvertHealthyToSick(newCases, entry);
        }

        var diseaseDeaths = state.Sickness > 70 ? Math.Max(1, (state.Sickness - 70) / 15) : 0;
        if (diseaseDeaths > 0)
        {
            state.ApplyDeath(diseaseDeaths, entry);
        }

        // Wounded recovery (before sick recovery, takes priority for medicine)
        if (GameBalance.EnableWoundedSystem)
        {
            state.Population.AdvanceWoundedRecoveryTimers();

            // Check for untreated wounded deaths
            var untreatedDeaths = state.Population.WoundedUntreatedDeaths(GameBalance.WoundedDeathDays);
            if (untreatedDeaths > 0)
            {
                state.Population.RemoveWoundedWorkers(untreatedDeaths);
                state.TotalDeaths += untreatedDeaths;
                entry.Write($"{untreatedDeaths} wounded died from untreated injuries.");
            }

            // Heal wounded with medicine (priority over sick)
            var woundedReady = state.Population.WoundedReadyToRecoverCount();
            if (woundedReady > 0)
            {
                var medAvailable = state.Resources[ResourceKind.Medicine];
                var canHeal = Math.Min(woundedReady, medAvailable / Math.Max(1, GameBalance.MedicinePerWoundedRecovery));
                if (canHeal > 0)
                {
                    var medCost = canHeal * GameBalance.MedicinePerWoundedRecovery;
                    state.Resources.Consume(ResourceKind.Medicine, medCost);
                    state.Population.RecoverWoundedWorkers(canHeal);
                    state.TotalRecoveredWorkers += canHeal;
                    entry.Write($"{canHeal} wounded recovered ({medCost} medicine used).");
                }
            }
        }

        if (state.Sickness < GameBalance.RecoveryThresholdSickness)
        {
            report.RecoveryEnabledToday = true;
            state.Population.AdvanceRecoveryTimers();
            entry.Write("Recovery timers advanced.");

            var ready = state.Population.ReadyToRecoverCount();
            var baseClinicCap = production.ClinicSlotsUsed * GameBalance.RecoveryPerClinicSlot;
            var clinicCap = GameBalance.EnableClinicSpecialization && state.ClinicSpecialization == ClinicSpecialization.Hospital
                ? (int)Math.Floor(baseClinicCap * (1 + GameBalance.HospitalRecoveryBonus))
                : baseClinicCap;
            var medicinePerRecovery = GameBalance.MedicinePerRecovery * state.DailyEffects.MedicineUsageMultiplier.Value;
            var medicineCap = medicinePerRecovery <= 0
                ? ready
                : (int)Math.Floor(state.Resources[ResourceKind.Medicine] / medicinePerRecovery);

            var recoveries = Math.Min(ready, Math.Min(clinicCap, medicineCap));
            if (recoveries > 0)
            {
                var medicineSpend = (int)Math.Ceiling(recoveries * medicinePerRecovery);
                state.Resources.Consume(ResourceKind.Medicine, medicineSpend);
                report.RecoveryMedicineSpentToday += medicineSpend;
                state.RecoverSickWorkers(recoveries, report, entry);
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
                    entry.Write("Recovery blocked by medicine shortage.");
                }
            }
        }
        else
        {
            report.RecoveryEnabledToday = false;
            report.RecoveryBlockedReason =
                $"Global sickness is {state.Sickness}, recovery requires below {GameBalance.RecoveryThresholdSickness}.";
            entry.Write(report.RecoveryBlockedReason);
        }
    }

    static void ApplyUnrestProgression(GameState state, ResolutionEntry entry)
    {
        var unrestDeltaTracked = StatModifiers.ComputeUnrestProgression(state);
        var unrestDelta = unrestDeltaTracked.Value;
        if (state.FoodDeficitToday)
        {
            unrestDelta += 2;
        }

        if (state.WaterDeficitToday)
        {
            unrestDelta += 2;
        }

        var moraleBonusFromSpecs = 0;
        if (GameBalance.EnableBuildingSpecializations)
        {
            if (state.GetBuildingSpec(BuildingId.Storehouse) == BuildingSpecialization.WeaponCache
                && !state.GetBuilding(BuildingId.Storehouse).IsDestroyed)
                unrestDelta -= GameBalance.WeaponCacheUnrestReduction;

            if (state.GetBuildingSpec(BuildingId.RationingPost) == BuildingSpecialization.PropagandaPost
                && !state.GetBuilding(BuildingId.RationingPost).IsDestroyed)
            {
                unrestDelta += GameBalance.PropagandaPostDailyUnrest;
                moraleBonusFromSpecs += GameBalance.PropagandaPostDailyMorale;
            }

            if (state.GetBuildingSpec(BuildingId.FieldKitchen) == BuildingSpecialization.SoupLine
                && !state.GetBuilding(BuildingId.FieldKitchen).IsDestroyed
                && state.GetBuilding(BuildingId.FieldKitchen).AssignedWorkers > 0)
                moraleBonusFromSpecs += GameBalance.SoupLineDailyMorale;
        }

        if (GameBalance.EnableNamedCharacters)
        {
            if (state.GetLivingCharacterWithTrait(CharacterTrait.Elder) is not null)
                unrestDelta -= 2;
        }

        // Humanity score effects on unrest
        if (GameBalance.EnableHumanityScore && state.Flags.Humanity < GameBalance.HumanityLowThreshold)
        {
            if (state.RollPercent() <= GameBalance.HumanityUnrestChance)
            {
                unrestDelta += 1;
                entry.Write("A dark pall hangs over the city. The people sense something has been lost.");
            }
        }

        if (unrestDelta != 0)
        {
            state.AddUnrest(unrestDelta, entry);
        }

        var moraleDeltaTracked = StatModifiers.ComputeMoraleDrift(state);
        var moraleTotal = moraleDeltaTracked.Value + moraleBonusFromSpecs;

        if (GameBalance.EnableNamedCharacters)
        {
            if (state.GetLivingCharacterWithTrait(CharacterTrait.Orator) is not null)
                moraleTotal += 2;
        }

        // Humanity score effects on morale
        if (GameBalance.EnableHumanityScore && state.Flags.Humanity >= GameBalance.HumanityHighThreshold)
        {
            if (state.RollPercent() <= GameBalance.HumanityMoraleBoostChance)
            {
                moraleTotal += GameBalance.HumanityMoraleBoostAmount;
            }
        }

        if (moraleTotal != 0)
        {
            state.AddMorale(moraleTotal, entry);
        }
    }

    static void ApplySiegeDamage(GameState state, ResolutionEntry entry)
    {
        if (state.SiegeEscalationDelayDays > 0)
        {
            state.SiegeEscalationDelayDays -= 1;
            entry.Write($"Night raid pressure delay active: {state.SiegeEscalationDelayDays} day(s) remaining.");
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
                entry.Write($"Siege intensity increased to {state.SiegeIntensity}.");
            }
        }

        var perimeter = state.ActivePerimeterZone;
        var perimeterFactor = ZoneRules.PerimeterFactor(state);
        var finalAssaultMultiplier = state.FinalAssaultActive ? 1.5 : 1.0;
        var dustStormMultiplier = state.DailyEffects.DustStormActive ? 0.8 : 1.0;
        var diplomaticSiegeMultiplier = state.DailyEffects.SiegeDamageMultiplier;
        var damage = (int)Math.Ceiling((GameBalance.PerimeterScalingBase + state.SiegeIntensity) * perimeterFactor *
                                       state.SiegeDamageMultiplier * finalAssaultMultiplier * dustStormMultiplier *
                                       diplomaticSiegeMultiplier);

        if (GameBalance.EnableDefensivePosture)
        {
            if (state.CurrentPosture == DefensivePosture.HunkerDown)
            {
                damage = Math.Max(1, (int)Math.Floor(damage * (1 - GameBalance.HunkerDownSiegeReduction)));
            }
            else if (state.CurrentPosture == DefensivePosture.ActiveDefense)
            {
                damage = Math.Max(1, (int)Math.Floor(damage * (1 - GameBalance.ActiveDefenseSiegeReduction)));
            }

            if (state.ScorchedPerimeterDamageReductionDays > 0)
            {
                damage = Math.Max(1, (int)Math.Floor(damage * (1 - GameBalance.ScorchedPerimeterSiegeReduction)));
                state.ScorchedPerimeterDamageReductionDays -= 1;
            }
        }

        if (GameBalance.EnableFortifications && perimeter.FortificationLevel > 0)
        {
            damage = Math.Max(1, damage - perimeter.FortificationLevel * GameBalance.FortificationDamageReductionPerLevel);
        }

        if (GameBalance.EnableDefenses)
        {
            var defenses = state.GetZoneDefenses(perimeter.Id);

            var archerPostActive = defenses.HasArcherPost
                && defenses.ArcherPostGuardsAssigned >= GameBalance.ArcherPostGuardsRequired
                && !(GameBalance.EnableDefensivePosture && state.AreGuardsCommitted);
            if (archerPostActive)
            {
                damage = Math.Max(1, (int)Math.Floor(damage * (1 - GameBalance.ArcherPostDamageReduction)));
            }

            if (defenses.HasOilCauldron)
            {
                defenses.HasOilCauldron = false;
                entry.Write($"Oil cauldron poured on attackers in {perimeter.Name}! Siege damage negated for today.");
                damage = 0;
            }

            if (damage > 0 && defenses.BarricadeBuffer > 0)
            {
                var absorbed = Math.Min(damage, defenses.BarricadeBuffer);
                defenses.BarricadeBuffer -= absorbed;
                damage -= absorbed;
                entry.Write($"Barricades in {perimeter.Name} absorbed {absorbed} damage ({defenses.BarricadeBuffer} buffer remaining).");
            }
        }

        perimeter.Integrity -= damage;
        entry.Write($"Siege struck {perimeter.Name}: -{damage} integrity.");

        if (perimeter.Integrity <= 0)
        {
            state.LoseZone(perimeter.Id, false, entry);
        }

        if (state.SiegeIntensity >= 4 && state.RollPercent() <= 15)
        {
            var foodRaid = (int)Math.Ceiling(state.Resources[ResourceKind.Food] * 0.15);
            var waterRaid = (int)Math.Ceiling(state.Resources[ResourceKind.Water] * 0.15);
            if (foodRaid > 0)
            {
                state.AddResource(ResourceKind.Food, -foodRaid, entry);
            }

            if (waterRaid > 0)
            {
                state.AddResource(ResourceKind.Water, -waterRaid, entry);
            }

            if (foodRaid > 0 || waterRaid > 0)
            {
                entry.Write($"Supply line raid: enemy forces destroyed {foodRaid} food and {waterRaid} water.");
            }
        }
    }

    static void ApplyRepairs(GameState state, DailyProductionResult production, ResolutionEntry entry)
    {
        var repairPoints = (int)Math.Round(production.RepairPoints * state.DailyEffects.RepairOutputMultiplier.Value);
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
            entry.Write($"Repair crews restored {applied} integrity to {perimeter.Name}.");
        }
    }

    public void ResolveNight(NightPlan plan)
    {
        if (!GameBalance.EnableNightPhase)
            return;

        state.LastNightResult = null;

        if (plan.SelectedLocationId is null)
            return;

        var location = state.AvailableScavengingLocations.FirstOrDefault(l => l.Id == plan.SelectedLocationId);
        if (location is null || location.VisitsRemaining <= 0)
            return;

        location.VisitsRemaining--;
        var workers = plan.AssignedWorkers;
        var deaths = 0;
        var narrative = "";

        // Danger roll
        if (state.RollPercent() <= location.CasualtyChancePercent)
        {
            deaths = Math.Min(location.MaxCasualties, workers);
            workers -= deaths;
        }

        // Calculate rewards scaled by workers sent
        var resourcesGained = new List<(ResourceKind Kind, int Amount)>();
        foreach (var reward in location.PossibleRewards)
        {
            var baseAmount = state.Random.Next(reward.Min, reward.Max + 1);
            var scaled = (int)(baseAmount * (plan.AssignedWorkers / (double)location.MaxWorkers));
            if (scaled > 0)
            {
                resourcesGained.Add((reward.Resource, scaled));
            }
        }

        // Apply casualties — split into dead + wounded
        var actualDeaths = deaths;
        var wounded = 0;
        if (deaths > 0 && GameBalance.EnableWoundedSystem)
        {
            actualDeaths = Math.Max(1, (int)(deaths * (1.0 - GameBalance.WoundedFromDeathsSplit)));
            wounded = deaths - actualDeaths;
        }

        if (actualDeaths > 0)
        {
            state.Population.RemoveHealthyWorkers(actualDeaths);
            state.TotalDeaths += actualDeaths;
            state.Allocation.RemoveWorkersProportionally(actualDeaths);
        }

        if (wounded > 0)
        {
            var actualWounded = state.Population.RemoveHealthyWorkers(wounded);
            if (actualWounded > 0)
            {
                state.Population.AddWoundedWorkers(actualWounded, GameBalance.WoundedBaseRecoveryDays);
                state.Allocation.RemoveWorkersProportionally(actualWounded);
            }
            wounded = actualWounded;
        }

        if (actualDeaths > 0 || wounded > 0)
        {
            narrative = $"The scavengers took casualties at {location.Name}. {actualDeaths} dead, {wounded} wounded.";
        }
        else
        {
            narrative = $"The scavengers returned safely from {location.Name}.";
        }

        // Apply resources
        foreach (var (kind, amount) in resourcesGained)
        {
            state.Resources.Add(kind, amount);
        }

        // Intel bonus
        var intelGained = false;
        if (location.ProvidesIntel)
        {
            state.IntelBuffDaysRemaining = Math.Max(state.IntelBuffDaysRemaining, 3);
            intelGained = true;
        }

        // Set fatigue — workers who returned alive and uninjured
        var returnedWorkers = plan.AssignedWorkers - actualDeaths - wounded;
        state.FatiguedWorkerCount = Math.Max(0, returnedWorkers);

        state.LastNightResult = new ScavengingResult
        {
            LocationName = location.Name,
            ResourcesGained = resourcesGained,
            Deaths = actualDeaths,
            Wounded = wounded,
            WorkersReturned = Math.Max(0, returnedWorkers),
            LocationDepleted = location.VisitsRemaining <= 0,
            Narrative = narrative,
            IntelGained = intelGained,
        };
    }

    void ApplyCharacterTraitBonuses()
    {
        foreach (var character in state.LivingCharacters())
        {
            switch (character.Trait)
            {
                case CharacterTrait.Herbalist:
                    state.DailyEffects.MedicineUsageMultiplier.Apply("Herbalist: " + character.Name, 0.90);
                    break;
                case CharacterTrait.Engineer:
                    state.DailyEffects.RepairProductionMultiplier.Apply("Engineer: " + character.Name, 1.10);
                    break;
                case CharacterTrait.Blacksmith:
                    state.DailyEffects.MaterialsProductionMultiplier.Apply("Blacksmith: " + character.Name, 1.10);
                    break;
                case CharacterTrait.Strategist:
                case CharacterTrait.Scout:
                case CharacterTrait.Merchant:
                    state.DailyEffects.MissionSuccessBonus += 0.05;
                    break;
                case CharacterTrait.FormerSoldier:
                    // Treated as +1 effective guard in guard checks
                    break;
                case CharacterTrait.Orator:
                case CharacterTrait.Elder:
                case CharacterTrait.Healer:
                    // Applied in ApplyUnrestProgression
                    break;
            }
        }
    }

    void TriggerEvents()
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

            if (evt is ITriggeredEvent trackedEvent && trackedEvent.IsOnCooldown(state))
            {
                continue;
            }

            if (!evt.ShouldTrigger(state))
            {
                continue;
            }

            state.DailyEffects.TriggeredEvents.Add(evt);
            evt.StartCooldown(state);
        }
    }

    public void ApplyEventResponses(DayResolutionReport report, EventResponseChoice choice)
    {
        var evt = state.DailyEffects.TriggeredEvents.Find(x => x.Id == choice.EventId);
        if (evt == null)
            return;
        
        state.DailyEffects.TriggeredEvents.Remove(evt);
        
        var entry = new ResolutionEntry { Title = evt.Name };
        report.Entries.Add(entry);
        if (evt is not IRespondableEvent resp)
        {
            evt.ResolveNow(state, entry);
            return;
        }

        resp.ResolveWithResponse(choice.ResponseId, state, entry);
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
            state.ActiveMissions.RemoveAt(index);

            if (definition is not null)
            {
                var missionEntry = new ResolutionEntry { Title = $"Mission Completed: {definition.Name}" };
                definition.ResolveOutcome(state, active, missionEntry);
                report.Entries.Add(missionEntry);
            }
        }
    }

    static void CheckLossConditions(GameState state, ResolutionEntry entry)
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
            entry.Write("Loss: Keep breached.");
            return;
        }

        if (state.Unrest > GameBalance.RevoltThreshold)
        {
            state.GameOver = true;
            state.GameOverCause = GameOverCause.Revolt;
            state.GameOverDetails = $"Unrest exceeded {GameBalance.RevoltThreshold}.";
            entry.Write("Loss: revolt overwhelmed governance.");
            return;
        }

        if (state.ConsecutiveBothFoodWaterZeroDays >= GameBalance.FoodWaterLossThresholdDays)
        {
            state.GameOver = true;
            state.GameOverCause = GameOverCause.TotalCollapse;
            state.GameOverDetails = "Food and water at zero for 2 consecutive days.";
            entry.Write("Loss: total collapse from sustained zero food and water.");
            return;
        }

        if (state is { Sickness: >= 90, Population.HealthyWorkers: < 8 })
        {
            state.GameOver = true;
            state.GameOverCause = GameOverCause.PandemicCollapse;
            state.GameOverDetails =
                $"Sickness at {state.Sickness} with only {state.Population.HealthyWorkers} healthy workers. The city cannot function.";
            entry.Write("Loss: pandemic collapse. Too few healthy workers remain.");
        }
    }

    static void FinalizeDay(GameState state)
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

        // Night phase location refresh
        if (GameBalance.EnableNightPhase)
        {
            if (state.AvailableScavengingLocations.Count == 0 ||
                state.Day - state.ScavengingRefreshDay >= GameBalance.ScavengingLocationRefreshDays)
            {
                state.AvailableScavengingLocations = ScavengingLocationPool.GenerateNightLocations(state);
                state.ScavengingRefreshDay = state.Day;
            }
        }

        var targetDay = GameBalance.EnableReliefArmy
            ? state.ActualReliefDay - state.ReliefAcceleration
            : GameBalance.TargetSurvivalDay;

        if (state is { GameOver: false } && state.Day >= targetDay)
        {
            state.Survived = true;
            state.GameOver = true;
            state.GameOverCause = GameOverCause.None;
            state.GameOverDetails = GameBalance.EnableReliefArmy
                ? "The relief army has arrived!"
                : $"Day {GameBalance.TargetSurvivalDay} reached.";
        }
    }
}