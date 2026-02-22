using System;
using System.Collections.Generic;
using System.Linq;
using Prot8.Constants;
using Prot8.Jobs;
using Prot8.Laws;
using Prot8.Missions;
using Prot8.Orders;
using Prot8.Resources;
using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Cli.ViewModels;

public static class GameStateToViewModels
{
    public static DayStartViewModel ToDayStartViewModel(GameState state, bool noShortcuts)
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
                Materials = state.Resources[ResourceKind.Materials]
            },
            Population = new PopulationViewModel
            {
                HealthyWorkers = state.Population.HealthyWorkers,
                Guards = state.Population.Guards,
                SickWorkers = state.Population.SickWorkers,
                Elderly = state.Population.Elderly
            },
            JobAssignments = jobs.Select(job => new JobAssignmentViewModel
            {
                Job = job,
                Workers = state.Allocation.Workers[job]
            }).ToList(),
            Zones = state.Zones.Select(zone => new ZoneViewModel
            {
                Id = zone.Id,
                Name = zone.Name,
                Integrity = zone.Integrity,
                Capacity = zone.Capacity,
                Population = zone.Population,
                IsLost = zone.IsLost
            }).ToList(),
            ActiveMissions = state.ActiveMissions.Select(m => new ActiveMissionViewModel
            {
                MissionName = m.MissionName,
                DaysRemaining = m.DaysRemaining,
                WorkerCost = m.WorkerCost
            }).ToList(),
            AvailableLaws = ToLawViewModels(state, noShortcuts),
            AvailableOrders = ToOrderViewModels(state, noShortcuts),
            AvailableMissions = ToMissionViewModels(state, noShortcuts),
            Jobs = ToJobReferenceViewModels(noShortcuts)
        };
    }

    public static PendingPlanViewModel ToPendingPlanViewModel(GameState state, JobAllocation allocation, TurnActionChoice action, bool noShortcuts)
    {
        var jobs = ActionAvailability.GetJobTypes();
        var jobAssignments = jobs.Select(job => new JobAssignmentViewModel
        {
            Job = job,
            Workers = allocation.Workers[job]
        }).ToList();

        string? actionType = null;
        string? actionName = null;
        string? actionZone = null;

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
                if (action.SelectedZoneForOrder.HasValue)
                {
                    actionZone = action.SelectedZoneForOrder.Value.ToString();
                }
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
            QueuedActionZone = actionZone
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
                Message = e.Message
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
            RecoveryBlockedReason = report.RecoveryBlockedReason
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
                Materials = state.Resources[ResourceKind.Materials]
            },
            FinalPopulation = new PopulationViewModel
            {
                HealthyWorkers = state.Population.HealthyWorkers,
                Guards = state.Population.Guards,
                SickWorkers = state.Population.SickWorkers,
                Elderly = state.Population.Elderly
            }
        };
    }

    private static IReadOnlyList<LawViewModel> ToLawViewModels(GameState state, bool noShortcuts)
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
                Tooltip = law.GetDynamicTooltip(state),
                IsActive = state.ActiveLawIds.Contains(law.Id)
            });
        }

        return result;
    }

    private static IReadOnlyList<OrderViewModel> ToOrderViewModels(GameState state, bool noShortcuts)
    {
        var available = ActionAvailability.GetAvailableOrders(state);
        var result = new List<OrderViewModel>();

        for (var index = 0; index < available.Count; index++)
        {
            var order = available[index];

            var validZones = ActionAvailability.GetValidZonesForOrder(state, order);

            result.Add(new OrderViewModel
            {
                Id = order.Id,
                Name = order.Name,
                Tooltip = order.GetDynamicTooltip(state),
                RequiresZoneSelection = order.RequiresZoneSelection,
                ValidZones = validZones
            });
        }

        return result;
    }

    private static IReadOnlyList<MissionViewModel> ToMissionViewModels(GameState state, bool noShortcuts)
    {
        var available = ActionAvailability.GetAvailableMissions(state);
        var result = new List<MissionViewModel>();

        for (var index = 0; index < available.Count; index++)
        {
            var mission = available[index];

            result.Add(new MissionViewModel
            {
                Id = mission.Id,
                Name = mission.Name,
                Tooltip = mission.GetDynamicTooltip(state),
                DurationDays = mission.DurationDays,
                WorkerCost = mission.WorkerCost
            });
        }

        return result;
    }

    private static IReadOnlyList<JobReferenceViewModel> ToJobReferenceViewModels(bool noShortcuts)
    {
        var jobs = ActionAvailability.GetJobTypes();
        var result = new List<JobReferenceViewModel>();

        for (var index = 0; index < jobs.Count; index++)
        {
            var job = jobs[index];
            var shortcut = noShortcuts ? "" : $"j{index + 1}: ";

            result.Add(new JobReferenceViewModel
            {
                Job = job,
                Shortcut = shortcut,
                Description = ComputeJobDescription(job)
            });
        }

        return result;
    }

    private static string ComputeJobDescription(JobType job)
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
