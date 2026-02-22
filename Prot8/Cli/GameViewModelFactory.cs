using Prot8.Cli.ViewModels;
using Prot8.Constants;
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
                Population = state.GetZonePopulation(),
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

        return new PendingPlanViewModel
        {
            QueuedActionType = actionType,
            QueuedActionName = actionName,
        };
    }

    public static DayReportViewModel ToDayReportViewModel(GameState state, DayResolutionReport report)
    {
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
        var available = ActionAvailability.GetAvailableLaws(state);
        var result = new List<LawViewModel>();

        for (var index = 0; index < available.Count; index++)
        {
            var law = available[index];

            result.Add(new LawViewModel
            {
                Id = law.Id,
                Name = law.Name,
                Tooltip = law.GetTooltip(state),
                IsActive = state.ActiveLawIds.Contains(law.Id),
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
}