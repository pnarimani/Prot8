using Prot8.Buildings;
using Prot8.Constants;
using Prot8.Events;
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
    public IReadOnlyList<OrderCooldownViewModel> OrderCooldowns { get; init; } = [];
    public IReadOnlyList<MissionViewModel> AvailableMissions { get; init; } = [];
    public IReadOnlyList<BuildingViewModel> Buildings { get; init; } = [];
    public IReadOnlyList<ZoneStorageViewModel> ZoneStorages { get; init; } = [];
    public PendingEvent? CurrentEvent { get; init; }

    public string? ThreatProjection { get; init; }
    public string? ProductionForecast { get; init; }
    public string? ZoneWarnings { get; init; }
    public string? MoodLine { get; init; }
    public string? DisruptionText { get; init; }

    public int LawCooldownDaysRemaining { get; init; }
    public IReadOnlyList<MissionCooldownViewModel> MissionCooldowns { get; init; } = [];
    public double GlobalProductionMultiplier { get; init; } = 1.0;
    public IReadOnlyList<MultiplierEntry> ProductionMultiplierBreakdown { get; init; } = [];
    public IReadOnlyList<MultiplierEntry> FoodConsumptionBreakdown { get; init; } = [];
    public IReadOnlyList<MultiplierEntry> WaterConsumptionBreakdown { get; init; } = [];
    public IReadOnlyList<DeltaEntry> MoraleDeltaBreakdown { get; init; } = [];
    public IReadOnlyList<DeltaEntry> UnrestDeltaBreakdown { get; init; } = [];
    public IReadOnlyList<DeltaEntry> SicknessDeltaBreakdown { get; init; } = [];
    public int SiegeEscalationDelayDays { get; init; }
    public int ConsecutiveFoodDeficitDays { get; init; }
    public int ConsecutiveWaterDeficitDays { get; init; }
    public int ConsecutiveBothZeroDays { get; init; }
    public int OvercrowdingStacks { get; init; }

    public IReadOnlyList<string> SituationAlerts { get; init; } = [];
    public int MoraleDelta { get; init; }
    public int UnrestDelta { get; init; }
    public int SicknessDelta { get; init; }

    public IReadOnlyList<ResourceKind> ResourcePriority { get; init; } = [];
    public WorkerAllocationMode AllocationMode { get; init; }

    public string CurrentPosture { get; init; } = "None";
    public bool AreGuardsCommitted { get; init; }

    public IReadOnlyList<DiplomacyViewModel> AvailableDiplomacy { get; init; } = [];
    public IReadOnlyList<string> ActiveDiplomacyNames { get; init; } = [];

    public TradeViewModel? Trading { get; init; }
    public string? ReliefArmyEstimate { get; init; }

    public IReadOnlyList<CharacterViewModel> NamedCharacters { get; init; } = [];
}

public sealed class TradeViewModel
{
    public bool TradingPostBuilt { get; init; }
    public int TradingPostWorkers { get; init; }
    public double CurrentRate { get; init; }
    public IReadOnlyList<string> StandingTrades { get; init; } = [];
}

public sealed class DiplomacyViewModel
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Tooltip { get; init; } = "";
    public bool IsActive { get; init; }
    public bool CanDeactivate { get; init; }
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
    public int FortificationLevel { get; init; }
    public int MaxFortificationLevel { get; init; }
    public int BarricadeBuffer { get; init; }
    public bool HasOilCauldron { get; init; }
    public bool HasArcherPost { get; init; }
    public int ArcherPostGuardsAssigned { get; init; }
}

public sealed class ActiveMissionViewModel
{
    public string MissionName { get; init; } = "";
    public int DaysRemaining { get; init; }
    public int WorkerCost { get; init; }
    public int GuardCost { get; init; }
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
    public int CooldownDays { get; init; }
    public int CooldownRemaining { get; init; }
    public bool IsOnCooldown { get; init; }
}

public sealed class OrderCooldownViewModel
{
    public string OrderName { get; init; } = "";
    public int DaysRemaining { get; init; }
}

public sealed class MissionViewModel
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Tooltip { get; init; } = "";
    public int DurationDays { get; init; }
    public int RequiredIdleWorkers { get; init; }
    public int GuardCost { get; init; }
}

public sealed class BuildingViewModel
{
    public required BuildingId Id { get; init; }
    public required string Name { get; init; }
    public required ZoneId Zone { get; init; }
    public required string ZoneName { get; init; }
    public required int MaxWorkers { get; init; }
    public required bool IsDestroyed { get; init; }
    public required bool IsActive { get; init; }
    public required int AssignedWorkers { get; init; }
    public required List<ResourceQuantity> CurrentInput { get; init; }
    public required List<ResourceQuantity> CurrentOutput { get; init; }
    public required List<ResourceQuantity> InputPerWorker { get; init; }
    public required List<ResourceQuantity> OutputPerWorker { get; init; }
    public int UpgradeLevel { get; init; }
    public int UpgradeDaysRemaining { get; init; }
    public int MaxUpgradeLevel { get; init; }
    public string? Specialization { get; init; }
}

public sealed class ZoneStorageViewModel
{
    public ZoneId ZoneId { get; init; }
    public string ZoneName { get; init; } = "";
    public int Level { get; init; }
    public int MaxLevel { get; init; }
    public int CapacityPerResource { get; init; }
    public int Food { get; init; }
    public int Water { get; init; }
    public int Fuel { get; init; }
    public int Medicine { get; init; }
    public int Materials { get; init; }
    public bool IsLost { get; init; }
}

public sealed class PendingPlanViewModel
{
    public string? QueuedActionType { get; init; }
    public string? QueuedActionName { get; init; }
}

public sealed class MissionCooldownViewModel
{
    public string MissionName { get; init; } = "";
    public int DaysRemaining { get; init; }
}

public sealed class DayReportViewModel
{
    public int Day { get; init; }
    public IReadOnlyList<ResolutionEntry> Entries { get; init; } = [];
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

public sealed class NightPhaseViewModel
{
    public IReadOnlyList<ScavengingLocationViewModel> Locations { get; init; } = [];
    public int AvailableWorkers { get; init; }
    public int MinWorkers { get; init; }
    public int MaxWorkers { get; init; }
}

public sealed class ScavengingLocationViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Danger { get; init; } = "";
    public int VisitsRemaining { get; init; }
    public string Rewards { get; init; } = "";
    public bool ProvidesIntel { get; init; }
}

public sealed class CharacterViewModel
{
    public string Name { get; init; } = "";
    public string TraitName { get; init; } = "";
    public string TraitEffect { get; init; } = "";
    public bool IsAlive { get; init; }
    public bool HasDeserted { get; init; }
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
    public int? HumanityScore { get; init; }
}
