using Prot8.Cli.ViewModels;
using Prot8.Constants;
using Prot8.Decrees;
using Prot8.Jobs;
using Prot8.Laws;
using Prot8.Missions;
using Prot8.Orders;
using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Cli;

public class GameViewModelFactory(GameState state)
{
    public DayStartViewModel Create()
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
            FoodConsumptionMultiplier = ComputeFoodConsumptionMultiplier(state),
            WaterConsumptionMultiplier = ComputeWaterConsumptionMultiplier(state),
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
            OrderCooldownDaysRemaining = ComputeOrderCooldown(state),
            AvailableMissions = ToMissionViewModels(state),
            Jobs = CreateJobViewModel(state),
            ThreatProjection = ComputeThreatProjection(state),
            ProductionForecast = ComputeProductionForecast(state),
            ZoneWarnings = ComputeZoneWarnings(state),
            MoodLine = ComputeMoodLine(state),
            AvailableDecrees = ToDecreeViewModels(state),
            DisruptionText = state.ActiveDisruption,
        };
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

        string? decreeType = null;
        string? decreeName = null;
        if (!string.IsNullOrWhiteSpace(action.DecreeId))
        {
            decreeType = "Decree";
            var decree = DecreeCatalog.Find(action.DecreeId);
            decreeName = decree?.Name ?? action.DecreeId;
        }

        return new PendingPlanViewModel
        {
            QueuedActionType = actionType,
            QueuedActionName = actionName,
            QueuedDecreeType = decreeType,
            QueuedDecreeName = decreeName,
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
        var deltaSummary = $"NET: Food {(foodDelta >= 0 ? "+" : "")}{foodDelta}  Water {(waterDelta >= 0 ? "+" : "")}{waterDelta}  Fuel {(fuelDelta >= 0 ? "+" : "")}{fuelDelta}  |  Morale {(moraleDelta >= 0 ? "+" : "")}{moraleDelta}  Unrest {(unrestDelta >= 0 ? "+" : "")}{unrestDelta}  Sickness {(sicknessDelta >= 0 ? "+" : "")}{sicknessDelta}";

        var workerDelta = state.Population.HealthyWorkers - report.StartHealthyWorkers;
        string? allocationAlert = null;
        if (workerDelta < 0)
        {
            allocationAlert = $"ATTENTION: {-workerDelta} workers removed from assignments due to population loss. Review allocations.";
        }

        return new DayReportViewModel
        {
            Day = report.Day,
            Entries = report.Entries.Select(e => new DeltaLogEntryViewModel
            {
                Tag = e.Tag,
                Message = e.Message,
            }).ToList(),
            TriggeredEvents = report.TriggeredEvents,
            ResolvedMissions = report.ResolvedMissions,
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

        // First add all enacted (active) laws
        foreach (var lawId in state.ActiveLawIds)
        {
            var law = LawCatalog.Find(lawId);
            if (law is null) continue;
            result.Add(new LawViewModel
            {
                Id = law.Id,
                Name = law.Name,
                Tooltip = law.GetTooltip(state),
                IsActive = true,
            });
        }

        // Then add available (enactable) laws
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

    static int ComputeOrderCooldown(GameState state)
    {
        if (state.LastOrderDay == int.MinValue)
        {
            return 0;
        }

        var remaining = GameBalance.OrderCooldownDays - (state.Day - state.LastOrderDay);
        return remaining > 0 ? remaining : 0;
    }

    static IReadOnlyList<OrderViewModel> ToOrderViewModels(GameState state)
    {
        var available = ActionAvailability.GetAvailableOrders(state);
        var result = new List<OrderViewModel>();

        foreach (var order in available)
        {
            result.Add(new OrderViewModel
            {
                Id = order.Id,
                Name = order.Name,
                Tooltip = order.GetTooltip(state),
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

    static Dictionary<JobType, JobViewModel> CreateJobViewModel(GameState state)
    {
        var jobs = ActionAvailability.GetJobTypes();
        var result = new Dictionary<JobType, JobViewModel>();

        foreach (var job in jobs)
        {
            var workers = state.Allocation.Workers[job];

            var output = GameBalance.JobOutputs[job]
                .Select(x => x with { Quantity = x.Quantity * workers })
                .ToList();

            var input = GameBalance.JobInputs[job]
                .Select(x => x with { Quantity = x.Quantity * workers })
                .ToList();

            result.Add(job, new JobViewModel
            {
                AssignedWorkers = workers,
                CurrentOutput = output,
                CurrentInput = input,
                InputPerWorker = GameBalance.JobInputs[job],
                OutputPerWorker = GameBalance.JobOutputs[job],
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
                parts.Add($"Food runs out in ~{foodDays} days");
        }

        if (waterNeed > 0 && state.Resources[ResourceKind.Water] > 0)
        {
            var waterDays = state.Resources[ResourceKind.Water] / waterNeed;
            if (waterDays <= 5)
                parts.Add($"Water runs out in ~{waterDays} days");
        }

        var unrestRate = StatModifiers.ComputeUnrestProgression(state);
        if (unrestRate > 0 && state.Unrest < GameBalance.RevoltThreshold)
        {
            var revoltDays = (GameBalance.RevoltThreshold - state.Unrest) / unrestRate;
            if (revoltDays <= 10)
                parts.Add($"Revolt in ~{revoltDays} days");
        }

        var perimeter = state.ActivePerimeterZone;
        if (!perimeter.IsLost && perimeter.Integrity > 0)
        {
            var perimeterFactor = ZoneRules.PerimeterFactor(state);
            var dailyDamage = (int)Math.Ceiling((GameBalance.PerimeterScalingBase + state.SiegeIntensity) * perimeterFactor);
            if (dailyDamage > 0)
            {
                var zoneDays = perimeter.Integrity / dailyDamage;
                if (zoneDays <= 7)
                    parts.Add($"Zone fall in ~{zoneDays} days");
            }
        }

        return parts.Count > 0 ? "THREATS: " + string.Join(" | ", parts) : null;
    }

    static string? ComputeProductionForecast(GameState state)
    {
        var pop = state.Population.TotalPopulation;
        var foodNeed = (int)Math.Ceiling(pop * GameBalance.FoodPerPersonPerDay);
        var waterNeed = (int)Math.Ceiling(pop * GameBalance.WaterPerPersonPerDay);

        var foodWorkers = state.Allocation.Workers[Jobs.JobType.FoodProduction];
        var waterWorkers = state.Allocation.Workers[Jobs.JobType.WaterDrawing];
        var foodProd = foodWorkers * 2;
        var waterProd = waterWorkers * 2;

        var parts = new List<string>();
        if (foodWorkers > 0 || foodNeed > 0)
        {
            var delta = foodProd - foodNeed;
            parts.Add($"Food {(delta >= 0 ? "+" : "")}{delta}/day");
        }

        if (waterWorkers > 0 || waterNeed > 0)
        {
            var delta = waterProd - waterNeed;
            parts.Add($"Water {(delta >= 0 ? "+" : "")}{delta}/day");
        }

        return parts.Count > 0 ? "FORECAST: " + string.Join(" | ", parts) : null;
    }

    static string? ComputeZoneWarnings(GameState state)
    {
        var warnings = new List<string>();
        foreach (var zone in state.Zones)
        {
            if (zone.IsLost) continue;
            var template = GameBalance.ZoneTemplates.FirstOrDefault(t => t.ZoneId == zone.Id);
            if (template == null) continue;
            var threshold = (int)(template.StartingIntegrity * 0.3);
            if (zone.Integrity <= threshold && zone.Integrity > 0)
            {
                var perimeterFactor = ZoneRules.PerimeterFactor(state);
                var dailyDamage = (int)Math.Ceiling((GameBalance.PerimeterScalingBase + state.SiegeIntensity) * perimeterFactor);
                var estDays = dailyDamage > 0 ? zone.Integrity / dailyDamage : 99;
                warnings.Add($"!!! {zone.Name.ToUpper()} CRITICALLY DAMAGED ({zone.Integrity}/{template.StartingIntegrity}) — ESTIMATED FALL: {estDays} DAYS");
            }
        }

        return warnings.Count > 0 ? string.Join("\n", warnings) : null;
    }

    static string? ComputeMoodLine(GameState state)
    {
        if (state.Sickness > 60)
            return "The coughing never stops. Even the healthy avoid each other's eyes.";
        if (state.Unrest > 65)
            return "A guard was found beaten in an alley. Trust is eroding.";
        if (state.Morale < 25)
            return "Whispers of surrender echo through the streets.";
        if (state.Morale >= 50)
            return "People work in grim silence. They endure.";
        if (state.Unrest > 50)
            return "Arguments break out in the food lines. Tension hangs in the air.";
        if (state.Sickness > 40)
            return "The clinic is overcrowded. Families leave offerings at the temple.";
        return "Another dawn behind the walls. The city holds.";
    }

    static IReadOnlyList<DecreeViewModel> ToDecreeViewModels(GameState state)
    {
        var result = new List<DecreeViewModel>();
        foreach (var decree in DecreeCatalog.GetAll())
        {
            if (decree.CanIssue(state, out _))
            {
                result.Add(new DecreeViewModel
                {
                    Id = decree.Id,
                    Name = decree.Name,
                    Tooltip = decree.GetTooltip(state),
                });
            }
        }

        return result;
    }

    static double ComputeFoodConsumptionMultiplier(GameState state)
    {
        var multiplier = 1.0;
        if (state.ActiveLawIds.Contains("strict_rations"))
            multiplier *= 0.75;
        return multiplier;
    }

    static double ComputeWaterConsumptionMultiplier(GameState state)
    {
        var multiplier = 1.0;
        if (state.ActiveLawIds.Contains("diluted_water"))
            multiplier *= 0.75;
        return multiplier;
    }
}