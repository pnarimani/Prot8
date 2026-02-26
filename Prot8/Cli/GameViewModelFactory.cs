using Prot8.Buildings;
using Prot8.Cli.ViewModels;
using Prot8.Constants;
using Prot8.Events;
using Prot8.Laws;
using Prot8.Missions;
using Prot8.Mood;
using Prot8.Orders;
using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Cli;

public class GameViewModelFactory(GameState state)
{
    public DayStartViewModel CreateDayStartViewModel()
    {
        return new DayStartViewModel
        {
            Day = state.Day,
            TargetSurvivalDay = GameBalance.TargetSurvivalDay,
            SiegeIntensity = state.SiegeIntensity,
            ActivePerimeterName = state.ActivePerimeterZone.Name,
            Morale = state.Morale,
            Unrest = state.Unrest,
            Sickness = state.Sickness,
            IdleWorkersForAssignment = state.IdleWorkers,
            FoodConsumptionMultiplier = state.DailyEffects.FoodConsumptionMultiplier.Value,
            WaterConsumptionMultiplier = state.DailyEffects.WaterConsumptionMultiplier.Value,
            Resources = new ResourceViewModel
            {
                Food = state.Resources[ResourceKind.Food],
                Water = state.Resources[ResourceKind.Water],
                Fuel = state.Resources[ResourceKind.Fuel],
                Medicine = state.Resources[ResourceKind.Medicine],
                Materials = state.Resources[ResourceKind.Materials],
            },
            Population = new PopulationViewModel
            {
                HealthyWorkers = state.Population.HealthyWorkers,
                Guards = state.Population.Guards,
                SickWorkers = state.Population.SickWorkers,
                Elderly = state.Population.Elderly,
                SickReadyToRecover = state.Population.ReadyToRecoverCount(),
                RecoveryDaysAtCurrentSickness = GameBalance.ComputeRecoveryDays(state.Sickness),
            },
            Zones = state.Zones.Select(zone => new ZoneViewModel
            {
                Id = zone.Id,
                Name = zone.Name,
                Integrity = zone.Integrity,
                Capacity = zone.Capacity,
                Population = zone.IsLost ? 0 : state.GetZonePopulation(),
                IsLost = zone.IsLost,
            }).ToList(),
            ActiveMissions = state.ActiveMissions.Select(m => new ActiveMissionViewModel
            {
                MissionName = m.MissionName,
                DaysRemaining = m.DaysRemaining,
                WorkerCost = m.WorkerCost,
            }).ToList(),
            AvailableLaws = ToLawViewModels(state),
            AvailableOrders = ToOrderViewModels(state),
            OrderCooldowns = ComputeOrderCooldowns(state),
            AvailableMissions = ToMissionViewModels(state),
            Buildings = CreateBuildingViewModels(state),
            ZoneStorages = CreateZoneStorageViewModels(state),
            ThreatProjection = ComputeThreatProjection(state),
            ProductionForecast = ComputeProductionForecast(state),
            ZoneWarnings = ComputeZoneWarnings(state),
            MoodLine = MoodSelector.Select(state),
            DisruptionText = state.ActiveDisruption,
            LawCooldownDaysRemaining = ComputeLawCooldown(state),
            MissionCooldowns = ComputeMissionCooldowns(state),
            GlobalProductionMultiplier = ComputeGlobalProductionMultiplierValue(state),
            ProductionMultiplierBreakdown = ComputeProductionMultiplierBreakdown(state),
            FoodConsumptionBreakdown = state.DailyEffects.FoodConsumptionMultiplier.Entries,
            WaterConsumptionBreakdown = state.DailyEffects.WaterConsumptionMultiplier.Entries,
            SiegeEscalationDelayDays = state.SiegeEscalationDelayDays,
            ConsecutiveFoodDeficitDays = state.ConsecutiveFoodDeficitDays,
            ConsecutiveWaterDeficitDays = state.ConsecutiveWaterDeficitDays,
            ConsecutiveBothZeroDays = state.ConsecutiveBothFoodWaterZeroDays,
            OvercrowdingStacks = ComputeOvercrowdingStacks(state),
            SituationAlerts = ComputeSituationAlerts(state),
            MoraleDelta = ComputeMoraleDelta(state, out var moraleDeltaBreakdown),
            MoraleDeltaBreakdown = moraleDeltaBreakdown,
            UnrestDelta = ComputeUnrestDelta(state, out var unrestDeltaBreakdown),
            UnrestDeltaBreakdown = unrestDeltaBreakdown,
            SicknessDelta = ComputeSicknessDelta(state, out var sicknessDeltaBreakdown),
            SicknessDeltaBreakdown = sicknessDeltaBreakdown,
            ResourcePriority = state.ResourcePriority,
            AllocationMode = GameBalance.AllocationMode,
            CurrentEvent = GetCurrentEvent(),
        };
    }

    PendingEvent? GetCurrentEvent()
    {
        if (state.DailyEffects.TriggeredEvents.FirstOrDefault() is { } evt)
        {
            if (evt is IRespondableEvent resp)
            {
                return new PendingEvent(evt, resp.GetResponses(state));
            }

            return new PendingEvent(evt);
        }

        return null;
    }

    public static PendingPlanViewModel ToPendingPlanViewModel(TurnActionChoice action)
    {
        string? actionType = null;
        string? actionName = null;

        if (action.HasAction)
        {
            if (!string.IsNullOrWhiteSpace(action.LawId))
            {
                actionType = "Law";
                var law = LawCatalog.Find(action.LawId);
                actionName = law?.Name ?? action.LawId;
            }
            else if (!string.IsNullOrWhiteSpace(action.EmergencyOrderId))
            {
                actionType = "Emergency Order";
                var order = EmergencyOrderCatalog.Find(action.EmergencyOrderId);
                actionName = order?.Name ?? action.EmergencyOrderId;
            }
            else if (!string.IsNullOrWhiteSpace(action.MissionId))
            {
                actionType = "Mission";
                var mission = MissionCatalog.Find(action.MissionId);
                actionName = mission?.Name ?? action.MissionId;
            }
        }

        return new PendingPlanViewModel
        {
            QueuedActionType = actionType,
            QueuedActionName = actionName,
        };
    }

    public static DayReportViewModel ToDayReportViewModel(GameState state, DayResolutionReport report)
    {
        var foodDelta = state.Resources[ResourceKind.Food] - report.StartFood;
        var waterDelta = state.Resources[ResourceKind.Water] - report.StartWater;
        var fuelDelta = state.Resources[ResourceKind.Fuel] - report.StartFuel;
        var moraleDelta = state.Morale - report.StartMorale;
        var unrestDelta = state.Unrest - report.StartUnrest;
        var sicknessDelta = state.Sickness - report.StartSickness;
        var deltaSummary =
            $"NET: Food {(foodDelta >= 0 ? "+" : "")}{foodDelta}  Water {(waterDelta >= 0 ? "+" : "")}{waterDelta}  Fuel {(fuelDelta >= 0 ? "+" : "")}{fuelDelta}  |  Morale {(moraleDelta >= 0 ? "+" : "")}{moraleDelta}  Unrest {(unrestDelta >= 0 ? "+" : "")}{unrestDelta}  Sickness {(sicknessDelta >= 0 ? "+" : "")}{sicknessDelta}";

        var workerDelta = state.Population.HealthyWorkers - report.StartHealthyWorkers;
        string? allocationAlert = null;
        if (workerDelta < 0)
        {
            allocationAlert =
                $"ATTENTION: {-workerDelta} workers removed from assignments due to population loss. Review allocations.";
        }

        return new DayReportViewModel
        {
            Day = report.Day,
            Entries = report.Entries,
            FoodConsumedToday = report.FoodConsumedToday,
            WaterConsumedToday = report.WaterConsumedToday,
            FoodDeficitToday = report.FoodDeficitToday,
            WaterDeficitToday = report.WaterDeficitToday,
            FuelDeficitToday = report.FuelDeficitToday,
            RecoveredWorkersToday = report.RecoveredWorkersToday,
            RecoveryMedicineSpentToday = report.RecoveryMedicineSpentToday,
            RecoveryEnabledToday = report.RecoveryEnabledToday,
            RecoveryBlockedReason = report.RecoveryBlockedReason,
            DeltaSummary = deltaSummary,
            AllocationAlert = allocationAlert,
        };
    }

    public static GameOverViewModel ToGameOverViewModel(GameState state)
    {
        return new GameOverViewModel
        {
            Survived = state.Survived,
            Cause = state.GameOverCause,
            Details = state.GameOverDetails,
            Day = state.Day,
            TotalDeaths = state.TotalDeaths,
            TotalDesertions = state.TotalDesertions,
            LostZones = state.CountLostZones(),
            FinalResources = new ResourceViewModel
            {
                Food = state.Resources[ResourceKind.Food],
                Water = state.Resources[ResourceKind.Water],
                Fuel = state.Resources[ResourceKind.Fuel],
                Medicine = state.Resources[ResourceKind.Medicine],
                Materials = state.Resources[ResourceKind.Materials],
            },
            FinalPopulation = new PopulationViewModel
            {
                HealthyWorkers = state.Population.HealthyWorkers,
                Guards = state.Population.Guards,
                SickWorkers = state.Population.SickWorkers,
                Elderly = state.Population.Elderly,
            },
        };
    }

    static IReadOnlyList<LawViewModel> ToLawViewModels(GameState state)
    {
        var result = new List<LawViewModel>();

        foreach (var lawId in state.ActiveLawIds)
        {
            var law = LawCatalog.Find(lawId);
            if (law is null)
            {
                continue;
            }

            result.Add(new LawViewModel
            {
                Id = law.Id,
                Name = law.Name,
                Tooltip = law.GetTooltip(state),
                IsActive = true,
            });
        }

        var available = ActionAvailability.GetAvailableLaws(state);
        foreach (var law in available)
        {
            result.Add(new LawViewModel
            {
                Id = law.Id,
                Name = law.Name,
                Tooltip = law.GetTooltip(state),
                IsActive = false,
            });
        }

        return result;
    }

    static int ComputeLawCooldown(GameState state)
    {
        if (state.LastLawDay == int.MinValue)
        {
            return 0;
        }

        var remaining = GameBalance.LawCooldownDays - (state.Day - state.LastLawDay);
        return remaining > 0 ? remaining : 0;
    }

    static IReadOnlyList<MissionCooldownViewModel> ComputeMissionCooldowns(GameState state)
    {
        var result = new List<MissionCooldownViewModel>();
        foreach (var (missionId, lastDay) in state.MissionCooldowns)
        {
            var remaining = GameBalance.MissionCooldownDays - (state.Day - lastDay);
            if (remaining > 0)
            {
                var mission = MissionCatalog.Find(missionId);
                result.Add(new MissionCooldownViewModel
                {
                    MissionName = mission?.Name ?? missionId,
                    DaysRemaining = remaining,
                });
            }
        }

        return result;
    }

    static int ComputeOvercrowdingStacks(GameState state)
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
        return overflow > 0 ? overflow / GameBalance.OvercrowdingThreshold : 0;
    }

    static IReadOnlyList<OrderCooldownViewModel> ComputeOrderCooldowns(GameState state)
    {
        var result = new List<OrderCooldownViewModel>();
        foreach (var order in EmergencyOrderCatalog.GetAll())
        {
            if (state.OrderCooldowns.TryGetValue(order.Id, out var lastDay))
            {
                var remaining = order.CooldownDays - (state.Day - lastDay);
                if (remaining > 0)
                {
                    result.Add(new OrderCooldownViewModel
                    {
                        OrderName = order.Name,
                        DaysRemaining = remaining,
                    });
                }
            }
        }

        return result;
    }

    static IReadOnlyList<OrderViewModel> ToOrderViewModels(GameState state)
    {
        var result = new List<OrderViewModel>();

        foreach (var order in EmergencyOrderCatalog.GetAll())
        {
            var onCooldown = false;
            var cooldownRemaining = 0;
            if (state.OrderCooldowns.TryGetValue(order.Id, out var lastDay))
            {
                var remaining = order.CooldownDays - (state.Day - lastDay);
                if (remaining > 0)
                {
                    onCooldown = true;
                    cooldownRemaining = remaining;
                }
            }

            if (onCooldown || !order.CanIssue(state, out _))
            {
                continue;
            }

            result.Add(new OrderViewModel
            {
                Id = order.Id,
                Name = order.Name,
                Tooltip = order.GetTooltip(state),
                CooldownDays = order.CooldownDays,
                CooldownRemaining = cooldownRemaining,
                IsOnCooldown = onCooldown,
            });
        }

        return result;
    }

    static IReadOnlyList<MissionViewModel> ToMissionViewModels(GameState state)
    {
        var available = ActionAvailability.GetAvailableMissions(state);
        var result = new List<MissionViewModel>();

        foreach (var mission in available)
        {
            result.Add(new MissionViewModel
            {
                Id = mission.Id,
                Name = mission.Name,
                Tooltip = mission.GetTooltip(state),
                DurationDays = mission.DurationDays,
                RequiredIdleWorkers = mission.WorkerCost,
            });
        }

        return result;
    }

    static IReadOnlyList<BuildingViewModel> CreateBuildingViewModels(GameState state)
    {
        var result = new List<BuildingViewModel>();

        foreach (var building in state.Buildings)
        {
            var workers = building.AssignedWorkers;
            var zoneName = state.GetZone(building.Zone).Name;

            var output = building.Outputs
                .Select(x => x with { Quantity = x.Quantity * workers })
                .ToList();

            var input = building.Inputs
                .Select(x => x with { Quantity = x.Quantity * workers })
                .ToList();

            result.Add(new BuildingViewModel
            {
                Id = building.Id,
                Name = building.Name,
                Zone = building.Zone,
                ZoneName = zoneName,
                MaxWorkers = building.MaxWorkers,
                IsActive = building.IsActive,
                IsDestroyed = building.IsDestroyed,
                AssignedWorkers = workers,
                CurrentOutput = output,
                CurrentInput = input,
                InputPerWorker = building.Inputs.ToList(),
                OutputPerWorker = building.Outputs.ToList(),
            });
        }

        return result;
    }

    static string? ComputeThreatProjection(GameState state)
    {
        var parts = new List<string>();
        var pop = state.Population.TotalPopulation;
        var foodNeed = (int)Math.Ceiling(pop * GameBalance.FoodPerPersonPerDay);
        var waterNeed = (int)Math.Ceiling(pop * GameBalance.WaterPerPersonPerDay);

        if (foodNeed > 0 && state.Resources[ResourceKind.Food] > 0)
        {
            var foodDays = state.Resources[ResourceKind.Food] / foodNeed;
            if (foodDays <= 5)
            {
                parts.Add($"Food runs out in ~{foodDays} days");
            }
        }

        if (waterNeed > 0 && state.Resources[ResourceKind.Water] > 0)
        {
            var waterDays = state.Resources[ResourceKind.Water] / waterNeed;
            if (waterDays <= 5)
            {
                parts.Add($"Water runs out in ~{waterDays} days");
            }
        }

        var unrestRate = Math.Max(0, StatModifiers.ComputeUnrestProgression(state).Value);
        if (unrestRate > 0 && state.Unrest < GameBalance.RevoltThreshold)
        {
            var revoltDays = (GameBalance.RevoltThreshold - state.Unrest) / unrestRate;
            if (revoltDays <= 10)
            {
                parts.Add($"Revolt in ~{revoltDays} days");
            }
        }

        var perimeter = state.ActivePerimeterZone;
        if (!perimeter.IsLost && perimeter.Integrity > 0)
        {
            var perimeterFactor = ZoneRules.PerimeterFactor(state);
            var dailyDamage =
                (int)Math.Ceiling((GameBalance.PerimeterScalingBase + state.SiegeIntensity) * perimeterFactor);
            if (dailyDamage > 0)
            {
                var zoneDays = perimeter.Integrity / dailyDamage;
                if (zoneDays <= 7)
                {
                    parts.Add($"Zone fall in ~{zoneDays} days");
                }
            }
        }

        return parts.Count > 0 ? "THREATS: " + string.Join(" | ", parts) : null;
    }

    static string? ComputeProductionForecast(GameState state)
    {
        var pop = state.Population.TotalPopulation;
        var foodNeed = (int)Math.Ceiling(pop * GameBalance.FoodPerPersonPerDay);
        var waterNeed = (int)Math.Ceiling(pop * GameBalance.WaterPerPersonPerDay);

        var foodProd = 0.0;
        var waterProd = 0.0;
        var anyFoodWorkers = false;
        var anyWaterWorkers = false;

        foreach (var building in state.GetActiveBuildings())
        {
            if (building.AssignedWorkers <= 0) continue;
            var output = building.Outputs.FirstOrDefault();
            if (output.Resource == ResourceKind.Food)
            {
                foodProd += building.AssignedWorkers * output.Quantity;
                anyFoodWorkers = true;
            }
            else if (output.Resource == ResourceKind.Water)
            {
                waterProd += building.AssignedWorkers * output.Quantity;
                anyWaterWorkers = true;
            }
        }

        var parts = new List<string>();
        if (anyFoodWorkers || foodNeed > 0)
        {
            var delta = (int)foodProd - foodNeed;
            parts.Add($"Food {(delta >= 0 ? "+" : "")}{delta}/day");
        }

        if (anyWaterWorkers || waterNeed > 0)
        {
            var delta = (int)waterProd - waterNeed;
            parts.Add($"Water {(delta >= 0 ? "+" : "")}{delta}/day");
        }

        return parts.Count > 0 ? "FORECAST: " + string.Join(" | ", parts) : null;
    }

    static string? ComputeZoneWarnings(GameState state)
    {
        var warnings = new List<string>();
        foreach (var zone in state.Zones)
        {
            if (zone.IsLost)
            {
                continue;
            }

            var template = GameBalance.ZoneTemplates.FirstOrDefault(t => t.ZoneId == zone.Id);
            if (template == null)
            {
                continue;
            }

            var threshold = (int)(template.StartingIntegrity * 0.3);
            if (zone.Integrity <= threshold && zone.Integrity > 0)
            {
                var perimeterFactor = ZoneRules.PerimeterFactor(state);
                var dailyDamage =
                    (int)Math.Ceiling((GameBalance.PerimeterScalingBase + state.SiegeIntensity) * perimeterFactor);
                var estDays = dailyDamage > 0 ? zone.Integrity / dailyDamage : 99;
                warnings.Add(
                    $"!!! {zone.Name.ToUpper()} CRITICALLY DAMAGED ({zone.Integrity}/{template.StartingIntegrity}) — ESTIMATED FALL: {estDays} DAYS");
            }
        }

        return warnings.Count > 0 ? string.Join("\n", warnings) : null;
    }

    static IReadOnlyList<string> ComputeSituationAlerts(GameState state)
    {
        var alerts = new List<string>();
        var pop = state.Population.TotalPopulation;

        var foodNeed = (int)Math.Ceiling(pop * GameBalance.FoodPerPersonPerDay);
        if (foodNeed > 0 && state.Resources[ResourceKind.Food] > 0)
        {
            var foodDays = state.Resources[ResourceKind.Food] / foodNeed;
            if (foodDays <= 1)
            {
                alerts.Add("CRITICAL: Food runs out tomorrow");
            }
            else if (foodDays <= 2)
            {
                alerts.Add("WARNING: Food runs out in 2 days");
            }
        }
        else if (state.Resources[ResourceKind.Food] == 0)
        {
            alerts.Add("CRITICAL: No food remaining");
        }

        var waterNeed = (int)Math.Ceiling(pop * GameBalance.WaterPerPersonPerDay);
        if (waterNeed > 0 && state.Resources[ResourceKind.Water] > 0)
        {
            var waterDays = state.Resources[ResourceKind.Water] / waterNeed;
            if (waterDays <= 1)
            {
                alerts.Add("CRITICAL: Water runs out tomorrow");
            }
            else if (waterDays <= 2)
            {
                alerts.Add("WARNING: Water runs out in 2 days");
            }
        }
        else if (state.Resources[ResourceKind.Water] == 0)
        {
            alerts.Add("CRITICAL: No water remaining");
        }

        if (state.Unrest >= 80)
        {
            alerts.Add("CRITICAL: Revolt imminent");
        }
        else if (state.Unrest >= 70)
        {
            alerts.Add("WARNING: Unrest dangerously high");
        }

        if (state.Sickness >= 80)
        {
            alerts.Add("CRITICAL: Pandemic collapse approaching");
        }
        else if (state.Sickness >= 60)
        {
            alerts.Add("WARNING: Sickness epidemic spreading");
        }

        var perimeter = state.ActivePerimeterZone;
        if (!perimeter.IsLost && perimeter.Integrity <= 15 && perimeter.Integrity > 0)
        {
            alerts.Add($"CRITICAL: {perimeter.Name} about to fall");
        }

        return alerts;
    }

    static double ComputeGlobalProductionMultiplierValue(GameState state)
    {
        var statMult = StatModifiers.ComputeGlobalProductionMultiplier(state);
        return Math.Clamp(statMult.Value, 0.25, 1.3) * state.DailyEffects.ProductionMultiplier.Value;
    }

    static IReadOnlyList<MultiplierEntry> ComputeProductionMultiplierBreakdown(GameState state)
    {
        var entries = new List<MultiplierEntry>();
        var statMult = StatModifiers.ComputeGlobalProductionMultiplier(state);
        entries.AddRange(statMult.Entries);
        entries.AddRange(state.DailyEffects.ProductionMultiplier.Entries);
        return entries;
    }

    static int ComputeMoraleDelta(GameState state, out IReadOnlyList<DeltaEntry> breakdown)
    {
        var tracked = StatModifiers.ComputeMoraleDrift(state);
        breakdown = tracked.Entries;
        return tracked.Value;
    }

    static int ComputeUnrestDelta(GameState state, out IReadOnlyList<DeltaEntry> breakdown)
    {
        var tracked = StatModifiers.ComputeUnrestProgression(state);
        breakdown = tracked.Entries;
        return Math.Max(0, tracked.Value);
    }

    static int ComputeSicknessDelta(GameState state, out IReadOnlyList<DeltaEntry> breakdown)
    {
        var tracked = StatModifiers.ComputeSicknessFromEnvironment(state);
        breakdown = tracked.Entries;
        return tracked.Value;
    }

    static IReadOnlyList<ZoneStorageViewModel> CreateZoneStorageViewModels(GameState state)
    {
        var result = new List<ZoneStorageViewModel>();
        foreach (var zs in state.Resources.ZoneStorages)
        {
            var zone = state.GetZone(zs.ZoneId);
            result.Add(new ZoneStorageViewModel
            {
                ZoneId = zs.ZoneId,
                ZoneName = zone.Name,
                Level = zs.UpgradeLevel,
                MaxLevel = GameBalance.StorageMaxUpgradeLevel,
                CapacityPerResource = zs.Capacity,
                Food = zs.GetStored(ResourceKind.Food),
                Water = zs.GetStored(ResourceKind.Water),
                Fuel = zs.GetStored(ResourceKind.Fuel),
                Medicine = zs.GetStored(ResourceKind.Medicine),
                Materials = zs.GetStored(ResourceKind.Materials),
                IsLost = zone.IsLost,
            });
        }
        return result;
    }
}