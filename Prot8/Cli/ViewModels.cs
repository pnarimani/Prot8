using Prot8.Jobs;
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
    
    public int IdleWorkersForAssignment { get; init; }

    public double FoodConsumptionMultiplier { get; init; } = 1.0;
    public double WaterConsumptionMultiplier { get; init; } = 1.0;

    public ResourceViewModel Resources { get; init; } = new();
    public PopulationViewModel Population { get; init; } = new();
    public IReadOnlyList<ZoneViewModel> Zones { get; init; } = [];
    public IReadOnlyList<ActiveMissionViewModel> ActiveMissions { get; init; } = [];
    public IReadOnlyList<LawViewModel> AvailableLaws { get; init; } = [];
    public IReadOnlyList<OrderViewModel> AvailableOrders { get; init; } = [];
    public int OrderCooldownDaysRemaining { get; init; }
    public IReadOnlyList<MissionViewModel> AvailableMissions { get; init; } = [];
    public Dictionary<JobType, JobViewModel> Jobs { get; init; } = [];

    public string? ThreatProjection { get; init; }
    public string? ProductionForecast { get; init; }
    public string? ZoneWarnings { get; init; }
    public string? MoodLine { get; init; }
    public string? DisruptionText { get; init; }
    public IReadOnlyList<DecreeViewModel> AvailableDecrees { get; init; } = [];

    public int LawCooldownDaysRemaining { get; init; }
    public IReadOnlyList<MissionCooldownViewModel> MissionCooldowns { get; init; } = [];
    public double GlobalProductionMultiplier { get; init; } = 1.0;
    public int SiegeEscalationDelayDays { get; init; }
    public int ConsecutiveFoodDeficitDays { get; init; }
    public int ConsecutiveWaterDeficitDays { get; init; }
    public int ConsecutiveBothZeroDays { get; init; }
    public int OvercrowdingStacks { get; init; }
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
    public int SickReadyToRecover { get; init; }
    public int RecoveryDaysAtCurrentSickness { get; init; }
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
    public int RequiredIdleWorkers { get; init; }
}

public sealed class JobViewModel
{
    public required int AssignedWorkers { get; init; }
    public required List<ResourceQuantity> CurrentInput { get; init; }
    public required List<ResourceQuantity> CurrentOutput { get; init; }
    public required List<ResourceQuantity> InputPerWorker { get; init; }
    public required List<ResourceQuantity> OutputPerWorker { get; init; }
}

public sealed class PendingPlanViewModel
{
    public string? QueuedActionType { get; init; }
    public string? QueuedActionName { get; init; }
    public string? QueuedDecreeType { get; init; }
    public string? QueuedDecreeName { get; init; }
}

public sealed class MissionCooldownViewModel
{
    public string MissionName { get; init; } = "";
    public int DaysRemaining { get; init; }
}

public sealed class DecreeViewModel
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Tooltip { get; init; } = "";
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
    public string? DeltaSummary { get; init; }
    public string? AllocationAlert { get; init; }
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