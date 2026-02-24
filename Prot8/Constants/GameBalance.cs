using Prot8.Jobs;
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
        new(ZoneId.OuterFarms, "Outer Farms", 70, 15, 1.0),
        new(ZoneId.OuterResidential, "Outer Residential", 75, 15, 0.9),
        new(ZoneId.ArtisanQuarter, "Artisan Quarter", 70, 15, 0.8),
        new(ZoneId.InnerDistrict, "Inner District", 65, 15, 0.7),
        new(ZoneId.Keep, "Keep", 60, 15, 0.6),
    };

    public static readonly IReadOnlyDictionary<JobType, ZoneId> JobZoneMap = new Dictionary<JobType, ZoneId>
    {
        [JobType.FoodProduction] = ZoneId.OuterFarms,
        [JobType.WaterDrawing] = ZoneId.OuterResidential,
        [JobType.MaterialsCrafting] = ZoneId.ArtisanQuarter,
        [JobType.Repairs] = ZoneId.Keep,
        [JobType.ClinicStaff] = ZoneId.InnerDistrict,
        [JobType.FuelScavenging] = ZoneId.OuterResidential,
    };

    public static readonly Dictionary<JobType, List<ResourceQuantity>> JobInputs = new()
    {
        [JobType.FoodProduction] = [new ResourceQuantity(ResourceKind.Fuel, 1)],
        [JobType.WaterDrawing] = [new ResourceQuantity(ResourceKind.Fuel, 1)],
        [JobType.MaterialsCrafting] = [],
        [JobType.Repairs] = [new ResourceQuantity(ResourceKind.Materials, 3)],
        [JobType.ClinicStaff] = [new ResourceQuantity(ResourceKind.Medicine, 1)],
        [JobType.FuelScavenging] = [],
    };

    public static readonly Dictionary<JobType, List<ResourceQuantity>> JobOutputs = new()
    {
        [JobType.FoodProduction] = [new ResourceQuantity(ResourceKind.Food, 3)],
        [JobType.WaterDrawing] = [new ResourceQuantity(ResourceKind.Water, 3)],
        [JobType.MaterialsCrafting] = [new ResourceQuantity(ResourceKind.Materials, 2)],
        [JobType.Repairs] = [new ResourceQuantity(ResourceKind.Integrity, 1d )],
        [JobType.ClinicStaff] = [new ResourceQuantity(ResourceKind.Care, 1)],
        [JobType.FuelScavenging] = [new ResourceQuantity(ResourceKind.Fuel, 2)],
    };

    public static readonly Dictionary<JobType, double> LostZoneJobMultipliers = new()
    {
        [JobType.FoodProduction] = 0.35,
        [JobType.WaterDrawing] = 0.6,
        [JobType.MaterialsCrafting] = 0.45,
        [JobType.Repairs] = 1.0,
        [JobType.ClinicStaff] = 0.65,
        [JobType.FuelScavenging] = 0.6,
    };

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