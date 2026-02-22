using Prot8.Constants;
using Prot8.Jobs;
using Prot8.Laws;
using Prot8.Missions;
using Prot8.Orders;
using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Cli.ViewModels;

public static class GameStateToViewModels
{
    public static DayStartViewModel ToDayStartViewModel(GameState state)
    {
        var jobs = ActionAvailability.GetJobTypes();

        return new DayStartViewModel
        {
            Day = state.Day,
            TargetSurvivalDay = GameBalance.TargetSurvivalDay,
            SiegeIntensity = state.SiegeIntensity,
            ActivePerimeterName = state.ActivePerimeterZone.Name,
            Morale = state.Morale,
            Unrest = state.Unrest,
            Sickness = state.Sickness,
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
            JobAssignments = jobs.Select(job => new JobAssignmentViewModel
            {
                Job = job,
                Workers = state.Allocation.Workers[job],
            }).ToList(),
            Zones = state.Zones.Select(zone => new ZoneViewModel
            {
                Id = zone.Id,
                Name = zone.Name,
                Integrity = zone.Integrity,
                Capacity = zone.Capacity,
                Population = zone.Population,
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
            AvailableMissions = ToMissionViewModels(state),
            Jobs = ToJobReferenceViewModels(),
        };
    }

    public static PendingPlanViewModel ToPendingPlanViewModel(GameState state, JobAllocation allocation,
        TurnActionChoice action)
    {
        var jobs = ActionAvailability.GetJobTypes();
        var jobAssignments = jobs.Select(job => new JobAssignmentViewModel
        {
            Job = job,
            Workers = allocation.Workers[job],
        }).ToList();

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
            JobAssignments = jobAssignments,
            TotalAssigned = allocation.TotalAssigned(),
            AvailableWorkers = state.AvailableHealthyWorkersForAllocation,
            IdleWorkers = allocation.IdleWorkers,
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

    public static GameOverViewModel ToGameOverViewModel(GameState state) =>
        new()
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
                WorkerCost = mission.WorkerCost,
            });
        }

        return result;
    }

    static IReadOnlyList<JobReferenceViewModel> ToJobReferenceViewModels()
    {
        var jobs = ActionAvailability.GetJobTypes();
        var result = new List<JobReferenceViewModel>();

        foreach (var job in jobs)
        {
            result.Add(new JobReferenceViewModel
            {
                Job = job,
                Description = ComputeJobDescription(job),
            });
        }

        return result;
    }

    static string ComputeJobDescription(JobType job)
    {
        var baseOutput = GameBalance.BaseJobOutputPerSlot[job];
        var outputResource = GameBalance.JobOutputResource[job];

        var outputDesc = outputResource.HasValue
            ? $"+{baseOutput} {outputResource.Value}"
            : job == JobType.Repairs
                ? $"+{baseOutput} integrity"
                : $"+{baseOutput} care pts";

        if (!GameBalance.JobInputPerSlot.TryGetValue(job, out var inputs) || inputs.Count == 0)
            return $"{job} ({outputDesc}).";

        var inputParts = inputs.Select(kvp => $"-{kvp.Value} {kvp.Key}");
        return $"{job} ({outputDesc}), {string.Join(", ", inputParts)}/slot).";
    }
}