using Prot8.Buildings;
using Prot8.Resources;
using Prot8.Zones;

namespace Prot8.Constants;

public enum WorkerAllocationMode
{
    ManualAssignment,
    AutoAllocation,
    PriorityQueue,
    BuildingActivation,
}

public static class GameBalance
{
    public static WorkerAllocationMode AllocationMode => WorkerAllocationMode.ManualAssignment;

    public static readonly IReadOnlyList<ResourceKind> DefaultResourcePriority =
    [
        ResourceKind.Food, ResourceKind.Water, ResourceKind.Fuel,
        ResourceKind.Materials, ResourceKind.Integrity, ResourceKind.Medicine, ResourceKind.Care,
    ];

    public const int TargetSurvivalDay = 40;

    public const int StartingHealthyWorkers = 60;
    public const int StartingGuards = 15;
    public const int StartingSickWorkers = 15;
    public const int StartingElderly = 15;

    public const int StartingFood = 150;
    public const int StartingWater = 150;
    public const int StartingFuel = 145;
    public const int StartingMedicine = 30;
    public const int StartingMaterials = 100;

    public const int StartingMorale = 60;
    public const int StartingUnrest = 25;
    public const int StartingSickness = 20;
    public const int StartingSiegeIntensity = 1;

    public const int MaxSiegeIntensity = 6;

    public const int RevoltThreshold = 85;
    public const int FoodWaterLossThresholdDays = 2;

    public const int OvercrowdingThreshold = 4;
    public const int OvercrowdingUnrestPerStack = 3;
    public const int OvercrowdingSicknessPerStack = 3;
    public const double OvercrowdingConsumptionPerStack = 0.05;

    public const int EvacIntegrityThreshold = 35;
    public const int EvacSiegeThreshold = 4;

    public const int LawCooldownDays = 0;

    public const int MissionCooldownDays = 4;

    public const double FoodPerPersonPerDay = 0.45;
    public const double WaterPerPersonPerDay = 0.40;
    public const double FuelPerPersonPerDay = 0.15;

    public const double PerimeterScalingBase = 5.0;

    public const int SiegeEscalationIntervalDays = 4;

    public const int RecoveryThresholdSickness = 60;
    public const int BaseRecoveryTimeDays = 4;
    public const int RecoveryPerClinicSlot = 2;
    public const int MedicinePerRecovery = 1;

    // Building Upgrades
    public static bool EnableBuildingUpgrades => true;
    public const int BuildingMaxUpgradeLevel = 3;
    public const int BuildingUpgradeMaterialsCost = 30;
    public const double BuildingUpgradeBonusPerLevel = 0.5;
    public const int BuildingUpgradeDelayDays = 1;

    // Kitchen Recipes
    public static bool EnableKitchenRecipes => true;
    public const double GruelFoodPerWorker = 1.5;
    public const int GruelSicknessPerDay = 1;
    public const double FeastFuelPerWorker = 2;
    public const double FeastFoodPerWorker = 3.0;
    public const int FeastMoralePerDay = 3;

    // Sortie Mission
    public static bool EnableSortieMission => true;
    public const int SortieGuardCost = 5;
    public const int SortieSuccessChance = 30;
    public const int SortiePartialChance = 35;
    public const int SortieSuccessSiegeReduction = 1;
    public const int SortieSuccessEscalationDelay = 3;
    public const double SortiePartialDamageMultiplier = 0.8;
    public const int SortiePartialDurationDays = 3;
    public const int SortieFailGuardDeaths = 3;
    public const int SortieFailUnrest = 10;

    // Defenses
    public static bool EnableDefenses => true;
    public const int BarricadeMaterialsCost = 15;
    public const int BarricadeBufferAmount = 15;
    public const int OilCauldronFuelCost = 10;
    public const int OilCauldronMaterialsCost = 10;
    public const int ArcherPostMaterialsCost = 20;
    public const int ArcherPostGuardsRequired = 2;
    public const double ArcherPostDamageReduction = 0.15;

    // Scouting Mission
    public static bool EnableScoutingMission => true;
    public const int ScoutingSuccessChance = 55;
    public const int ScoutingFailDeaths = 2;
    public const int ScoutingFailUnrest = 5;

    // Spy Intel Event
    public static bool EnableSpyIntelEvent => true;
    public const int SpyIntelMinDay = 8;
    public const int SpyIntelTriggerChance = 15;
    public const int SpyIntelMaterialsCost = 15;
    public const int SpyIntelFoodCost = 10;
    public const int IntelBuffDurationDays = 5;
    public const double IntelMissionSuccessBonus = 0.10;
    public const int IntelInterceptGuardCost = 3;
    public const int IntelInterceptGuardDeathRisk = 2;
    public const double IntelInterceptSiegeDamageReduction = 0.7;
    public const int IntelInterceptDurationDays = 3;
    public const int IntelBraceIntegrityBonus = 10;

    // Black Market Event
    public static bool EnableBlackMarketEvent => true;
    public const int BlackMarketMinDay = 5;
    public const int BlackMarketRecurrenceMin = 5;
    public const int BlackMarketRecurrenceMax = 7;
    public const int BlackMarketHaggleUnrest = 5;

    // Clinic Specialization
    public static bool EnableClinicSpecialization => true;
    public const double HospitalRecoveryBonus = 0.5;
    public const int QuarantineWardSicknessReduction = 5;
    public const int ClinicSpecializationMaterialsCost = 25;

    // Flag System
    public static bool EnableFlagSystem => true;

    // Fortifications
    public static bool EnableFortifications => true;
    public const int FortificationMaxLevel = 3;
    public const int FortificationMaterialsCost = 20;
    public const int FortificationDamageReductionPerLevel = 2;

    // Storage system
    public const int StorageBaseCapacity = 80;
    public const int StorageCapacityPerUpgrade = 40;
    public const int StorageMaxUpgradeLevel = 3;
    public const int StorageUpgradeMaterialsCost = 25;
    public static bool WasteExcessResources => true;
    public const double EvacuationResourceSalvagePercent = 0.0;

    // Building Specializations
    public static bool EnableBuildingSpecializations => true;
    public const int BuildingSpecializationMaterialsCost = 25;

    // Farm specs
    public const double GrainSilosFoodPerWorker = 4;
    public const double MedicinalHerbsFoodPerWorker = 2;
    public const double MedicinalHerbsMedicinePerWorker = 0.5;

    // HerbGarden specs
    public const double ApothecaryLabMedicinePerWorker = 1.5;
    public const int ApothecaryLabFuelInput = 1;
    public const int HealersRefugeSicknessReduction = 3;

    // Well specs
    public const double DeepBoringWaterPerWorker = 4;
    public const int DeepBoringFuelInput = 2;
    public const int PurificationBasinSicknessReduction = 2;

    // FuelStore specs
    public const double CoalPitsFuelPerWorker = 3;
    public const int CoalPitsDailySickness = 1;
    public const double RationedDistributionFuelPerWorker = 1.5;
    public const double RationedDistributionFuelConsumptionMultiplier = 0.85;

    // FieldKitchen specs
    public const double SoupLineFoodPerWorker = 3;
    public const int SoupLineDailyMorale = -3;

    // Workshop specs
    public const double ArmsFoundryMaterialsPerWorker = 3;
    public const int ArmsFoundryFuelInput = 1;
    public const int SalvageYardChance = 10;
    public const int SalvageYardAmount = 5;

    // Smithy specs
    public const double WarSmithIntegrityPerWorker = 2;
    public const int WarSmithMaterialsInput = 3;
    public const double SmithyDefaultIntegrityPerWorker = 1;

    // Cistern specs
    public const double RainCollectionWaterPerWorker = 1.5;
    public const double RainCollectionHeavyRainsMultiplier = 2.0;

    // Storehouse specs
    public const int WeaponCacheUnrestReduction = 5;
    public const double EmergencySuppliesSalvagePercent = 0.50;

    // RootCellar specs
    public const double PreservedStoresFoodPerWorker = 1.5;
    public const double PreservedStoresFoodConsumptionMultiplier = 0.90;
    public const double MushroomFarmFoodPerWorker = 2;
    public const int MushroomFarmDailySickness = 1;

    // RepairYard specs
    public const double SiegeWorkshopIntegrityPerWorker = 2;
    public const int SiegeWorkshopMaterialsInput = 4;
    public const double EngineerCorpsFortificationCostMultiplier = 0.50;

    // RationingPost specs
    public const double DistributionHubWaterPerWorker = 1.5;
    public const double DistributionHubFoodConsumptionMultiplier = 0.95;
    public const int PropagandaPostDailyMorale = 3;
    public const int PropagandaPostDailyUnrest = -2;

    // Defensive Posture System
    public static bool EnableDefensivePosture => true;
    public static bool EnableDefensivePostureGuardOverride => false;
    public const int DefensivePostureGuardMinimum = 3;
    public const double HunkerDownSiegeReduction = 0.25;
    public const double ActiveDefenseSiegeReduction = 0.35;
    public const int AggressivePatrolsUnrest = 5;
    public const int AggressivePatrolsInterceptChance = 15;
    public const int AggressivePatrolsResourceMin = 1;
    public const int AggressivePatrolsResourceMax = 6;
    public const int OpenGatesMorale = 5;
    public const int OpenGatesRefugeeChance = 20;
    public const int OpenGatesRefugeeMin = 3;
    public const int OpenGatesRefugeeMax = 6;
    public const int OpenGatesInfiltratorChance = 10;
    public const int OpenGatesInfiltratorUnrest = 8;
    public const int OpenGatesInfiltratorSickness = 3;
    public const int ScorchedPerimeterIntegrityDamage = 10;
    public const double ScorchedPerimeterSiegeReduction = 0.40;
    public const int ScorchedPerimeterDuration = 2;
    public const int ScorchedPerimeterMorale = -10;
    public const int ScorchedPerimeterTyranny = 1;

    // Morale Emergency Orders
    public static bool EnableMoraleOrders => true;

    public const int HoldAFeastFoodCost = 15;
    public const int HoldAFeastFuelCost = 5;
    public const int HoldAFeastMoraleGain = 20;
    public const int HoldAFeastUnrest = -10;
    public const int HoldAFeastFoodGate = 30;
    public const int HoldAFeastCooldown = 5;

    public const int DayOfRemembranceMoraleGain = 25;
    public const int DayOfRemembranceUnrest = -8;
    public const int DayOfRemembranceSickness = -5;
    public const int DayOfRemembranceFaithGain = 2;
    public const int DayOfRemembranceMoraleGate = 30;
    public const int DayOfRemembranceCooldown = 8;

    public const int PublicTrialDeaths = 2;
    public const int PublicTrialTyrannyUnrest = -25;
    public const int PublicTrialTyrannyMorale = -10;
    public const int PublicTrialFaithMorale = 10;
    public const int PublicTrialFaithUnrest = -10;
    public const int PublicTrialCooldown = 4;

    public const int StorytellingNightMoraleGain = 8;
    public const int StorytellingNightMoraleMin = 20;
    public const int StorytellingNightMoraleMax = 60;
    public const int StorytellingNightCooldown = 3;

    public const int DistributeLuxuriesFuelCost = 10;
    public const int DistributeLuxuriesMaterialsCost = 10;
    public const int DistributeLuxuriesMoraleGain = 15;
    public const int DistributeLuxuriesUnrest = -5;
    public const int DistributeLuxuriesSickness = -3;
    public const int DistributeLuxuriesMaterialsGate = 20;
    public const int DistributeLuxuriesFuelGate = 15;
    public const int DistributeLuxuriesCooldown = 5;

    // Cannibalism Law
    public static bool EnableCannibalismLaw => true;
    public const int CannibalismFoodThreshold = 5;
    public const int CannibalismTyrannyGain = 3;
    public const int CannibalismFearGain = 2;
    public const int CannibalismOnEnactUnrest = 20;
    public const int CannibalismOnEnactDesertions = 5;
    public const int CannibalismFoodPerDeath = 3;
    public const int CannibalismMaxFoodPerDay = 10;
    public const int CannibalismDailyMorale = -5;
    public const int CannibalismDailySickness = 3;
    public const int CannibalismDailyUnrest = -3;
    public const int CannibalismGuardDesertionChance = 15;
    public const int CannibalismWorkerDesertionChance = 10;

    // Production penalty thresholds - penalties only apply after these values
    public const int MoraleProductionThreshold = 65;
    public const int UnrestProductionThreshold = 65;
    public const int SicknessProductionThreshold = 65;

    public static readonly IReadOnlyList<ZoneTemplate> ZoneTemplates = new List<ZoneTemplate>
    {
        new(ZoneId.OuterFarms, "Outer Farms", 70, 25, 1.0),
        new(ZoneId.OuterResidential, "Outer Residential", 75, 25, 0.9),
        new(ZoneId.ArtisanQuarter, "Artisan Quarter", 70, 25, 0.8),
        new(ZoneId.InnerDistrict, "Inner District", 65, 25, 0.7),
        new(ZoneId.Keep, "Keep", 60, 25, 0.6),
    };

    public static readonly IReadOnlyList<BuildingDefinition> BuildingDefinitions =
    [
        // OUTER FARMS (lost first)
        new(BuildingId.Farm, "Farm", ZoneId.OuterFarms, 10,
            [new ResourceQuantity(ResourceKind.Fuel, 1)],
            [new ResourceQuantity(ResourceKind.Food, 3)]),
        new(BuildingId.HerbGarden, "Herb Garden", ZoneId.OuterFarms, 6,
            [],
            [new ResourceQuantity(ResourceKind.Medicine, 1)]),

        // OUTER RESIDENTIAL (lost second)
        new(BuildingId.Well, "Well", ZoneId.OuterResidential, 10,
            [new ResourceQuantity(ResourceKind.Fuel, 1)],
            [new ResourceQuantity(ResourceKind.Water, 3)]),
        new(BuildingId.FuelStore, "Fuel Store", ZoneId.OuterResidential, 8,
            [],
            [new ResourceQuantity(ResourceKind.Fuel, 2)]),
        new(BuildingId.FieldKitchen, "Field Kitchen", ZoneId.OuterResidential, 6,
            [new ResourceQuantity(ResourceKind.Fuel, 1)],
            [new ResourceQuantity(ResourceKind.Food, 2)]),

        // ARTISAN QUARTER (lost third)
        new(BuildingId.Workshop, "Workshop", ZoneId.ArtisanQuarter, 8,
            [],
            [new ResourceQuantity(ResourceKind.Materials, 2)]),
        new(BuildingId.Smithy, "Smithy", ZoneId.ArtisanQuarter, 6,
            [new ResourceQuantity(ResourceKind.Materials, 2)],
            [new ResourceQuantity(ResourceKind.Integrity, 1)]),
        new(BuildingId.Cistern, "Cistern", ZoneId.ArtisanQuarter, 6,
            [],
            [new ResourceQuantity(ResourceKind.Water, 1)]),

        // INNER DISTRICT (lost fourth)
        new(BuildingId.Clinic, "Clinic", ZoneId.InnerDistrict, 8,
            [new ResourceQuantity(ResourceKind.Medicine, 1)],
            [new ResourceQuantity(ResourceKind.Care, 1)]),
        new(BuildingId.Storehouse, "Storehouse", ZoneId.InnerDistrict, 6,
            [],
            [new ResourceQuantity(ResourceKind.Fuel, 1)]),
        new(BuildingId.RootCellar, "Root Cellar", ZoneId.InnerDistrict, 4,
            [],
            [new ResourceQuantity(ResourceKind.Food, 1)]),

        // KEEP (never lost)
        new(BuildingId.RepairYard, "Repair Yard", ZoneId.Keep, 8,
            [new ResourceQuantity(ResourceKind.Materials, 3)],
            [new ResourceQuantity(ResourceKind.Integrity, 1)]),
        new(BuildingId.RationingPost, "Rationing Post", ZoneId.Keep, 4,
            [],
            [new ResourceQuantity(ResourceKind.Water, 1)]),
    ];

    public static readonly IReadOnlyDictionary<ZoneId, int> NaturalLossUnrestShock = new Dictionary<ZoneId, int>
    {
        [ZoneId.OuterFarms] = 8,
        [ZoneId.OuterResidential] = 10,
        [ZoneId.ArtisanQuarter] = 12,
        [ZoneId.InnerDistrict] = 15,
        [ZoneId.Keep] = 0,
    };

    public static readonly IReadOnlyDictionary<ZoneId, int> NaturalLossMoraleShock = new Dictionary<ZoneId, int>
    {
        [ZoneId.OuterFarms] = 6,
        [ZoneId.OuterResidential] = 8,
        [ZoneId.ArtisanQuarter] = 10,
        [ZoneId.InnerDistrict] = 14,
        [ZoneId.Keep] = 0,
    };

    public static readonly IReadOnlyDictionary<ZoneId, int> NaturalLossSicknessShock = new Dictionary<ZoneId, int>
    {
        [ZoneId.OuterFarms] = 4,
        [ZoneId.OuterResidential] = 5,
        [ZoneId.ArtisanQuarter] = 7,
        [ZoneId.InnerDistrict] = 9,
        [ZoneId.Keep] = 0,
    };

    public static readonly IReadOnlyDictionary<ZoneId, int> EvacuationUnrestShock = new Dictionary<ZoneId, int>
    {
        [ZoneId.OuterFarms] = 6,
        [ZoneId.OuterResidential] = 8,
        [ZoneId.ArtisanQuarter] = 10,
        [ZoneId.InnerDistrict] = 12,
        [ZoneId.Keep] = 0,
    };

    public static readonly IReadOnlyDictionary<ZoneId, int> EvacuationMoraleShock = new Dictionary<ZoneId, int>
    {
        [ZoneId.OuterFarms] = 5,
        [ZoneId.OuterResidential] = 6,
        [ZoneId.ArtisanQuarter] = 8,
        [ZoneId.InnerDistrict] = 12,
        [ZoneId.Keep] = 0,
    };

    public static readonly IReadOnlyDictionary<ZoneId, int> EvacuationSicknessShock = new Dictionary<ZoneId, int>
    {
        [ZoneId.OuterFarms] = 3,
        [ZoneId.OuterResidential] = 4,
        [ZoneId.ArtisanQuarter] = 5,
        [ZoneId.InnerDistrict] = 7,
        [ZoneId.Keep] = 0,
    };

    public static readonly IReadOnlyDictionary<ZoneId, int> EvacuationMaterialsPenalty = new Dictionary<ZoneId, int>
    {
        [ZoneId.OuterFarms] = 10,
        [ZoneId.OuterResidential] = 15,
        [ZoneId.ArtisanQuarter] = 30,
        [ZoneId.InnerDistrict] = 20,
        [ZoneId.Keep] = 0,
    };

    public static readonly IReadOnlyDictionary<string, int> EventCooldownDays = new Dictionary<string, int>
    {
        ["hunger_riot"] = 3,
        ["fever_outbreak"] = 3,
        ["desertion_wave"] = 2,
        ["wall_breach_attempt"] = 2,
        ["fire_artisan"] = 4,
        ["council_revolt"] = 0,
        ["total_collapse"] = 0,
        ["siege_bombardment"] = 3,
        ["despair"] = 4,
        ["plague_rats"] = 0,
        ["enemy_ultimatum"] = 0,
        ["opening_bombardment"] = 0,
        ["supply_carts_intercepted"] = 0,
        ["refugees_at_gates"] = 0,
        ["enemy_sappers"] = 0,
        ["tainted_well"] = 0,
        ["final_assault"] = 0,
        ["betrayal_within"] = 0,
        ["smuggler_gate"] = 0,
        ["well_contamination"] = 0,
        ["militia_volunteers"] = 0,
        ["narrative_messenger"] = 0,
        ["narrative_towers"] = 0,
        ["narrative_letter"] = 0,
        ["narrative_burning_farms"] = 0,
        ["narrative_horns"] = 0,
        ["black_market_trading"] = 5,
        ["spy_selling_intel"] = 5,
        ["intel_siege_warning"] = 0,
        ["dissidents_discovered"] = 5,
        ["childrens_plea"] = 5,
        ["tyrants_reckoning"] = 0,
        ["siege_engineers_arrive"] = 5,
        ["crisis_of_faith"] = 0,
    };

    public static int ComputeRecoveryDays(int sickness)
    {
        if (sickness <= 19)
        {
            return BaseRecoveryTimeDays;
        }

        if (sickness <= 29)
        {
            return BaseRecoveryTimeDays + 1;
        }

        if (sickness <= 39)
        {
            return BaseRecoveryTimeDays + 2;
        }

        if (sickness <= 49)
        {
            return BaseRecoveryTimeDays + 3;
        }

        return 999;
    }

    public static int ClampStat(int value)
    {
        return Math.Clamp(value, 0, 100);
    }
}