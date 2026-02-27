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
    public static WorkerAllocationMode AllocationMode => WorkerAllocationMode.BuildingActivation;

    public static readonly IReadOnlyList<ResourceKind> DefaultResourcePriority =
    [
        ResourceKind.Food, ResourceKind.Water, ResourceKind.Fuel,
        ResourceKind.Materials, ResourceKind.Integrity, ResourceKind.Medicine, ResourceKind.Care,
    ];

    public const int TargetSurvivalDay = 40;

    public const int StartingHealthyWorkers = 60;
    public const int StartingGuards = 15;
    public const int StartingSickWorkers = 12;
    public const int StartingElderly = 15;

    public const int StartingFood = 170;
    public const int StartingWater = 180;
    public const int StartingFuel = 170;
    public const int StartingMedicine = 35;
    public const int StartingMaterials = 110;

    public const int StartingMorale = 58;
    public const int StartingUnrest = 22;
    public const int StartingSickness = 18;
    public const int StartingSiegeIntensity = 1;

    public const int MaxSiegeIntensity = 7;

    public const int RevoltThreshold = 88;
    public const int FoodWaterLossThresholdDays = 3;

    public const int OvercrowdingThreshold = 5;
    public const int OvercrowdingUnrestPerStack = 2;
    public const int OvercrowdingSicknessPerStack = 2;
    public const double OvercrowdingConsumptionPerStack = 0.04;

    public const int EvacIntegrityThreshold = 45;
    public const int EvacSiegeThreshold = 3;

    public const int LawCooldownDays = 2;

    public const int MissionCooldownDays = 3;

    public const double FoodPerPersonPerDay = 0.42;
    public const double WaterPerPersonPerDay = 0.36;
    public const double FuelPerPersonPerDay = 0.12;

    public const double PerimeterScalingBase = 4.5;

    public const int SiegeEscalationIntervalDays = 3;

    public const int RecoveryThresholdSickness = 65;
    public const int BaseRecoveryTimeDays = 4;
    public const int RecoveryPerClinicSlot = 2;
    public const int MedicinePerRecovery = 1;

    // Building Upgrades
    public static bool EnableBuildingUpgrades => true;
    public const int BuildingMaxUpgradeLevel = 3;
    public const int BuildingUpgradeMaterialsCost = 35;
    public const double BuildingUpgradeBonusPerLevel = 0.35;
    public const int BuildingUpgradeDelayDays = 2;

    // Kitchen Recipes
    public static bool EnableKitchenRecipes => true;
    public const double GruelFoodPerWorker = 1.3;
    public const int GruelSicknessPerDay = 2;
    public const double FeastFuelPerWorker = 2.5;
    public const double FeastFoodPerWorker = 2.8;
    public const int FeastMoralePerDay = 2;

    // Sortie Mission
    public static bool EnableSortieMission => true;
    public const int SortieGuardCost = 5;
    public const int SortieSuccessChance = 25;
    public const int SortiePartialChance = 40;
    public const int SortieSuccessSiegeReduction = 1;
    public const int SortieSuccessEscalationDelay = 2;
    public const double SortiePartialDamageMultiplier = 0.85;
    public const int SortiePartialDurationDays = 2;
    public const int SortieFailGuardDeaths = 4;
    public const int SortieFailUnrest = 12;

    // Defenses
    public static bool EnableDefenses => true;
    public const int BarricadeMaterialsCost = 15;
    public const int BarricadeBufferAmount = 12;
    public const int OilCauldronFuelCost = 10;
    public const int OilCauldronMaterialsCost = 10;
    public const int ArcherPostMaterialsCost = 20;
    public const int ArcherPostGuardsRequired = 2;
    public const double ArcherPostDamageReduction = 0.12;

    // Scouting Mission
    public static bool EnableScoutingMission => true;
    public const int ScoutingSuccessChance = 50;
    public const int ScoutingFailDeaths = 2;
    public const int ScoutingFailUnrest = 6;

    // Spy Intel Event
    public static bool EnableSpyIntelEvent => false;
    public const int SpyIntelMinDay = 8;
    public const int SpyIntelTriggerChance = 12;
    public const int SpyIntelMaterialsCost = 15;
    public const int SpyIntelFoodCost = 10;
    public const int IntelBuffDurationDays = 4;
    public const double IntelMissionSuccessBonus = 0.08;
    public const int IntelInterceptGuardCost = 3;
    public const int IntelInterceptGuardDeathRisk = 3;
    public const double IntelInterceptSiegeDamageReduction = 0.75;
    public const int IntelInterceptDurationDays = 2;
    public const int IntelBraceIntegrityBonus = 8;

    // Black Market Event
    public static bool EnableBlackMarketEvent => false;
    public const int BlackMarketMinDay = 5;
    public const int BlackMarketRecurrenceMin = 6;
    public const int BlackMarketRecurrenceMax = 8;
    public const int BlackMarketHaggleUnrest = 6;

    // Clinic Specialization
    public static bool EnableClinicSpecialization => true;
    public const double HospitalRecoveryBonus = 0.35;
    public const int QuarantineWardSicknessReduction = 4;
    public const int ClinicSpecializationMaterialsCost = 30;

    // Flag System
    public static bool EnableFlagSystem => true;

    // Fortifications
    public static bool EnableFortifications => true;
    public const int FortificationMaxLevel = 3;
    public const int FortificationMaterialsCost = 25;
    public const int FortificationDamageReductionPerLevel = 1;

    // Storage system
    public const int StorageBaseCapacity = 90;
    public const int StorageCapacityPerUpgrade = 35;
    public const int StorageMaxUpgradeLevel = 3;
    public const int StorageUpgradeMaterialsCost = 30;
    public static bool WasteExcessResources => true;
    public const double EvacuationResourceSalvagePercent = 0.25;

    // Trading Post
    public static bool EnableTradingPost => true;
    public const int TradingPostBuildCost = 45;
    public const double TradingPostBaseRate = 2.0;
    public const double TradingPostHighSiegeRate = 3.2;
    public const double TradingPostFluctuationRange = 0.25;
    public const int TradingPostInterceptionBase = 8;
    public const double TradingPostTyrannyRate = 1.4;
    public const int TradingPostTyrannyUnrestInterval = 3;
    public const int TradingPostTyrannyUnrest = 5;
    public const int TradingPostFaithBonusChance = 12;
    public const int TradingPostFaithBonusAmount = 2;

    // Diplomacy & Negotiation
    public static bool EnableDiplomacy => true;
    public const int BribeFoodCost = 10;
    public const int BribeMaterialsCost = 7;
    public const int BribeFoodCostTyranny = 7;
    public const int BribeMaterialsCostTyranny = 5;
    public const double BribeSiegeDamageMultiplier = 0.85;
    public const int BribeInterceptionChance = 10;
    public const int BribeInterceptionUnrest = 12;

    public const int HostageFoodCost = 4;
    public const int HostageMedicineCost = 2;
    public const int HostageDailyMorale = -3;

    public const int TributeFoodCost = 12;
    public const int TributeWaterCost = 12;
    public const int TributeDailyMorale = -6;

    public const int CorrespondenceMaterialsCost = 4;
    public const int CorrespondenceDailyMorale = 1;
    public const int CorrespondenceIntelChance = 8;
    public const int CorrespondenceIntelResourceAmount = 4;

    public const int BetrayalFood = 30;
    public const int BetrayalWater = 30;
    public const int BetrayalMaterials = 20;
    public const int BetrayalUnrest = 18;
    public const int BetrayalMorale = -18;
    public const int BetrayalRetaliationChance = 15;

    // Building Specializations
    public static bool EnableBuildingSpecializations => true;
    public const int BuildingSpecializationMaterialsCost = 25;

    // Farm specs
    public const double GrainSilosFoodPerWorker = 3.5;
    public const double MedicinalHerbsFoodPerWorker = 2;
    public const double MedicinalHerbsMedicinePerWorker = 0.5;

    // HerbGarden specs
    public const double ApothecaryLabMedicinePerWorker = 1.5;
    public const int ApothecaryLabFuelInput = 1;
    public const int HealersRefugeSicknessReduction = 3;

    // Well specs
    public const double DeepBoringWaterPerWorker = 3.5;
    public const int DeepBoringFuelInput = 2;
    public const int PurificationBasinSicknessReduction = 2;

    // FuelStore specs
    public const double CoalPitsFuelPerWorker = 2.5;
    public const int CoalPitsDailySickness = 1;
    public const double RationedDistributionFuelPerWorker = 1.5;
    public const double RationedDistributionFuelConsumptionMultiplier = 0.85;

    // FieldKitchen specs
    public const double SoupLineFoodPerWorker = 2.5;
    public const int SoupLineDailyMorale = -3;

    // Workshop specs
    public const double ArmsFoundryMaterialsPerWorker = 2.5;
    public const int ArmsFoundryFuelInput = 1;
    public const int SalvageYardChance = 10;
    public const int SalvageYardAmount = 5;

    // Smithy specs
    public const double WarSmithIntegrityPerWorker = 1.5;
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
    public const double SiegeWorkshopIntegrityPerWorker = 1.5;
    public const int SiegeWorkshopMaterialsInput = 4;
    public const double EngineerCorpsFortificationCostMultiplier = 0.50;

    // RationingPost specs
    public const double DistributionHubWaterPerWorker = 1.3;
    public const double DistributionHubFoodConsumptionMultiplier = 0.95;
    public const int PropagandaPostDailyMorale = 2;
    public const int PropagandaPostDailyUnrest = -1;

    // Defensive Posture System
    public static bool EnableDefensivePosture => true;
    public static bool EnableDefensivePostureGuardOverride => true;
    public const int DefensivePostureGuardMinimum = 4;
    public const double HunkerDownSiegeReduction = 0.20;
    public const double ActiveDefenseSiegeReduction = 0.30;
    public const int AggressivePatrolsUnrest = 2;
    public const int AggressivePatrolsInterceptChance = 12;
    public const int AggressivePatrolsResourceMin = 1;
    public const int AggressivePatrolsResourceMax = 5;
    public const int OpenGatesMorale = 3;
    public const int OpenGatesRefugeeChance = 15;
    public const int OpenGatesRefugeeMin = 2;
    public const int OpenGatesRefugeeMax = 5;
    public const int OpenGatesInfiltratorChance = 15;
    public const int OpenGatesInfiltratorUnrest = 10;
    public const int OpenGatesInfiltratorSickness = 4;
    public const int ScorchedPerimeterIntegrityDamage = 12;
    public const double ScorchedPerimeterSiegeReduction = 0.35;
    public const int ScorchedPerimeterDuration = 2;
    public const int ScorchedPerimeterMorale = -12;
    public const int ScorchedPerimeterTyranny = 1;

    // Morale Emergency Orders
    public static bool EnableMoraleOrders => true;

    public const int HoldAFeastFoodCost = 20;
    public const int HoldAFeastFuelCost = 8;
    public const int HoldAFeastMoraleGain = 15;
    public const int HoldAFeastUnrest = -6;
    public const int HoldAFeastFoodGate = 45;
    public const int HoldAFeastCooldown = 6;

    public const int DayOfRemembranceMoraleGain = 18;
    public const int DayOfRemembranceUnrest = -5;
    public const int DayOfRemembranceSickness = -3;
    public const int DayOfRemembranceFaithGain = 2;
    public const int DayOfRemembranceMoraleGate = 30;
    public const int DayOfRemembranceCooldown = 10;

    public const int PublicTrialDeaths = 2;
    public const int PublicTrialTyrannyUnrest = -18;
    public const int PublicTrialTyrannyMorale = -12;
    public const int PublicTrialFaithMorale = 8;
    public const int PublicTrialFaithUnrest = -7;
    public const int PublicTrialCooldown = 5;

    public const int StorytellingNightMoraleGain = 6;
    public const int StorytellingNightMoraleMin = 20;
    public const int StorytellingNightMoraleMax = 60;
    public const int StorytellingNightCooldown = 4;

    public const int DistributeLuxuriesFuelCost = 12;
    public const int DistributeLuxuriesMaterialsCost = 12;
    public const int DistributeLuxuriesMoraleGain = 10;
    public const int DistributeLuxuriesUnrest = -3;
    public const int DistributeLuxuriesSickness = -2;
    public const int DistributeLuxuriesMaterialsGate = 25;
    public const int DistributeLuxuriesFuelGate = 18;
    public const int DistributeLuxuriesCooldown = 6;

    // Cannibalism Law
    public static bool EnableCannibalismLaw => false;
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
    public static bool EnableProductionMultipliers => true;
    public const int MoraleProductionThreshold = 70;
    public const int UnrestProductionThreshold = 70;
    public const int SicknessProductionThreshold = 70;

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

        // INNER DISTRICT - constructable (starts inactive)
        new(BuildingId.TradingPost, "Trading Post", ZoneId.InnerDistrict, 4,
            [],
            []),
    ];

    public static readonly IReadOnlyDictionary<ZoneId, int> NaturalLossUnrestShock = new Dictionary<ZoneId, int>
    {
        [ZoneId.OuterFarms] = 6,
        [ZoneId.OuterResidential] = 8,
        [ZoneId.ArtisanQuarter] = 11,
        [ZoneId.InnerDistrict] = 14,
        [ZoneId.Keep] = 0,
    };

    public static readonly IReadOnlyDictionary<ZoneId, int> NaturalLossMoraleShock = new Dictionary<ZoneId, int>
    {
        [ZoneId.OuterFarms] = 5,
        [ZoneId.OuterResidential] = 7,
        [ZoneId.ArtisanQuarter] = 10,
        [ZoneId.InnerDistrict] = 13,
        [ZoneId.Keep] = 0,
    };

    public static readonly IReadOnlyDictionary<ZoneId, int> NaturalLossSicknessShock = new Dictionary<ZoneId, int>
    {
        [ZoneId.OuterFarms] = 3,
        [ZoneId.OuterResidential] = 4,
        [ZoneId.ArtisanQuarter] = 6,
        [ZoneId.InnerDistrict] = 8,
        [ZoneId.Keep] = 0,
    };

    public static readonly IReadOnlyDictionary<ZoneId, int> EvacuationUnrestShock = new Dictionary<ZoneId, int>
    {
        [ZoneId.OuterFarms] = 4,
        [ZoneId.OuterResidential] = 6,
        [ZoneId.ArtisanQuarter] = 8,
        [ZoneId.InnerDistrict] = 10,
        [ZoneId.Keep] = 0,
    };

    public static readonly IReadOnlyDictionary<ZoneId, int> EvacuationMoraleShock = new Dictionary<ZoneId, int>
    {
        [ZoneId.OuterFarms] = 3,
        [ZoneId.OuterResidential] = 5,
        [ZoneId.ArtisanQuarter] = 7,
        [ZoneId.InnerDistrict] = 10,
        [ZoneId.Keep] = 0,
    };

    public static readonly IReadOnlyDictionary<ZoneId, int> EvacuationSicknessShock = new Dictionary<ZoneId, int>
    {
        [ZoneId.OuterFarms] = 2,
        [ZoneId.OuterResidential] = 3,
        [ZoneId.ArtisanQuarter] = 4,
        [ZoneId.InnerDistrict] = 6,
        [ZoneId.Keep] = 0,
    };

    public static readonly IReadOnlyDictionary<ZoneId, int> EvacuationMaterialsPenalty = new Dictionary<ZoneId, int>
    {
        [ZoneId.OuterFarms] = 8,
        [ZoneId.OuterResidential] = 12,
        [ZoneId.ArtisanQuarter] = 24,
        [ZoneId.InnerDistrict] = 18,
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
