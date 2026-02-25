using Prot8.Buildings;
using Prot8.Resources;
using Prot8.Zones;

namespace Prot8.Constants;

public static class GameBalance
{
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
    public const double WaterPerPersonPerDay = 0.55;
    public const double FuelPerPersonPerDay = 0.08;

    public const double PerimeterScalingBase = 5.0;

    public const int SiegeEscalationIntervalDays = 4;

    public const int RecoveryThresholdSickness = 60;
    public const int BaseRecoveryTimeDays = 4;
    public const int RecoveryPerClinicSlot = 2;
    public const int MedicinePerRecovery = 1;

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