using System.Collections.Generic;
using Prot8.Jobs;
using Prot8.Laws;
using Prot8.Missions;
using Prot8.Orders;
using Prot8.Resources;
using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Cli.ViewModels;

public sealed class DayStartViewModel
{
    public int Day { get; init; }
    public int TargetSurvivalDay { get; init; }
    public int SiegeIntensity { get; init; }
    public string ActivePerimeterName { get; init; } = "";

    public int Morale { get; init; }
    public int Unrest { get; init; }
    public int Sickness { get; init; }

    public ResourceViewModel Resources { get; init; } = new();
    public PopulationViewModel Population { get; init; } = new();
    public IReadOnlyList<JobAssignmentViewModel> JobAssignments { get; init; } = [];
    public IReadOnlyList<ZoneViewModel> Zones { get; init; } = [];
    public IReadOnlyList<ActiveMissionViewModel> ActiveMissions { get; init; } = [];
    public IReadOnlyList<LawViewModel> AvailableLaws { get; init; } = [];
    public IReadOnlyList<OrderViewModel> AvailableOrders { get; init; } = [];
    public IReadOnlyList<MissionViewModel> AvailableMissions { get; init; } = [];
    public IReadOnlyList<JobReferenceViewModel> Jobs { get; init; } = [];
}

public sealed class ResourceViewModel
{
    public int Food { get; init; }
    public int Water { get; init; }
    public int Fuel { get; init; }
    public int Medicine { get; init; }
    public int Materials { get; init; }
}

public sealed class PopulationViewModel
{
    public int HealthyWorkers { get; init; }
    public int Guards { get; init; }
    public int SickWorkers { get; init; }
    public int Elderly { get; init; }
    public int TotalPopulation => HealthyWorkers + Guards + SickWorkers + Elderly;
}

public sealed class JobAssignmentViewModel
{
    public JobType Job { get; init; }
    public int Workers { get; init; }
}

public sealed class ZoneViewModel
{
    public ZoneId Id { get; init; }
    public string Name { get; init; } = "";
    public int Integrity { get; init; }
    public int Capacity { get; init; }
    public int Population { get; init; }
    public bool IsLost { get; init; }
}

public sealed class ActiveMissionViewModel
{
    public string MissionName { get; init; } = "";
    public int DaysRemaining { get; init; }
    public int WorkerCost { get; init; }
}

public sealed class LawViewModel
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Tooltip { get; init; } = "";
    public bool IsActive { get; init; }
}

public sealed class OrderViewModel
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Tooltip { get; init; } = "";
}

public sealed class MissionViewModel
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Tooltip { get; init; } = "";
    public int DurationDays { get; init; }
    public int WorkerCost { get; init; }
}

public sealed class JobReferenceViewModel
{
    public JobType Job { get; init; }
    public string Description { get; init; } = "";
}

public sealed class PendingPlanViewModel
{
    public IReadOnlyList<JobAssignmentViewModel> JobAssignments { get; init; } = [];
    public int TotalAssigned { get; init; }
    public int AvailableWorkers { get; init; }
    public int IdleWorkers { get; init; }
    public string? QueuedActionType { get; init; }
    public string? QueuedActionName { get; init; }
}

public sealed class DayReportViewModel
{
    public int Day { get; init; }
    public IReadOnlyList<DeltaLogEntryViewModel> Entries { get; init; } = [];
    public IReadOnlyList<string> TriggeredEvents { get; init; } = [];
    public IReadOnlyList<string> ResolvedMissions { get; init; } = [];
    public int FoodConsumedToday { get; init; }
    public int WaterConsumedToday { get; init; }
    public bool FoodDeficitToday { get; init; }
    public bool WaterDeficitToday { get; init; }
    public bool FuelDeficitToday { get; init; }
    public int RecoveredWorkersToday { get; init; }
    public int RecoveryMedicineSpentToday { get; init; }
    public bool RecoveryEnabledToday { get; init; }
    public string? RecoveryBlockedReason { get; init; }
}

public sealed class DeltaLogEntryViewModel
{
    public string Tag { get; init; } = "";
    public string Message { get; init; } = "";
}

public sealed class GameOverViewModel
{
    public bool Survived { get; init; }
    public GameOverCause Cause { get; init; }
    public string? Details { get; init; }
    public int Day { get; init; }
    public int TotalDeaths { get; init; }
    public int TotalDesertions { get; init; }
    public int LostZones { get; init; }
    public ResourceViewModel FinalResources { get; init; } = new();
    public PopulationViewModel FinalPopulation { get; init; } = new();
}
