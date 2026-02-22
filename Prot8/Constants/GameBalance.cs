using Prot8.Jobs;
using Prot8.Resources;
using Prot8.Zones;

namespace Prot8.Constants;

public static class GameBalance
{
    public const int TargetSurvivalDay = 40;

    public const int StartingHealthyWorkers = 85;
    public const int StartingGuards = 10;
    public const int StartingSickWorkers = 15;
    public const int StartingElderly = 10;

    public const int StartingFood = 320;
    public const int StartingWater = 360;
    public const int StartingFuel = 240;
    public const int StartingMedicine = 40;
    public const int StartingMaterials = 120;

    public const int StartingMorale = 55;
    public const int StartingUnrest = 25;
    public const int StartingSickness = 20;
    public const int StartingSiegeIntensity = 1;

    public const int MaxSiegeIntensity = 6;

    public const int RevoltThreshold = 85;
    public const int FoodWaterLossThresholdDays = 2;

    public const int OvercrowdingThreshold = 10;
    public const int OvercrowdingUnrestPerStack = 2;
    public const int OvercrowdingSicknessPerStack = 2;
    public const double OvercrowdingConsumptionPerStack = 0.05;

    public const int EvacIntegrityThreshold = 35;
    public const int EvacSiegeThreshold = 4;

    public const int LawCooldownDays = 3;

    public const double FoodPerPersonPerDay = 0.45;
    public const double WaterPerPersonPerDay = 0.55;
    public const double FuelPerPersonPerDay = 0.08;

    public const double PerimeterScalingBase = 4.0;

    public const int RecoveryThresholdSickness = 40;
    public const int BaseRecoveryTimeDays = 4;
    public const int RecoveryPerClinicSlot = 2;
    public const int MedicinePerRecovery = 1;

    public const int MissionForageWorkers = 10;
    public const int MissionNightRaidWorkers = 8;
    public const int MissionSearchHomesWorkers = 8;
    public const int MissionBlackMarketeersWorkers = 6;

    public static readonly IReadOnlyList<ZoneTemplate> ZoneTemplates = new List<ZoneTemplate>
    {
        new(ZoneId.OuterFarms, "Outer Farms", 100, 35, 30, 1.0),
        new(ZoneId.OuterResidential, "Outer Residential", 95, 30, 30, 0.9),
        new(ZoneId.ArtisanQuarter, "Artisan Quarter", 90, 25, 25, 0.8),
        new(ZoneId.InnerDistrict, "Inner District", 85, 20, 20, 0.7),
        new(ZoneId.Keep, "Keep", 80, 20, 15, 0.6),
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

    public static readonly IReadOnlyDictionary<JobType, double> BaseJobOutputPerSlot = new Dictionary<JobType, double>
    {
        [JobType.FoodProduction] = 14,
        [JobType.WaterDrawing] = 16,
        [JobType.MaterialsCrafting] = 10,
        [JobType.Repairs] = 3,
        [JobType.ClinicStaff] = 4,
        [JobType.FuelScavenging] = 8,
    };

    public static readonly Dictionary<JobType, Dictionary<ResourceKind, double>> JobInputPerSlot = new()
    {
        [JobType.FoodProduction] = new Dictionary<ResourceKind, double> { [ResourceKind.Water] = 2 },
        [JobType.WaterDrawing] = new Dictionary<ResourceKind, double> { [ResourceKind.Fuel] = 1 },
        [JobType.MaterialsCrafting] = new Dictionary<ResourceKind, double> { [ResourceKind.Fuel] = 2 },
        [JobType.Repairs] = new Dictionary<ResourceKind, double> { [ResourceKind.Materials] = 4, [ResourceKind.Fuel] = 2 },
        [JobType.ClinicStaff] = new Dictionary<ResourceKind, double> { [ResourceKind.Medicine] = 2 },
        [JobType.FuelScavenging] = new Dictionary<ResourceKind, double>(),
    };

    public static readonly IReadOnlyDictionary<JobType, ResourceKind?> JobOutputResource =
        new Dictionary<JobType, ResourceKind?>
        {
            [JobType.FoodProduction] = ResourceKind.Food,
            [JobType.WaterDrawing] = ResourceKind.Water,
            [JobType.MaterialsCrafting] = ResourceKind.Materials,
            [JobType.Repairs] = null,
            [JobType.ClinicStaff] = null,
            [JobType.FuelScavenging] = ResourceKind.Fuel,
        };

    public static readonly IReadOnlyDictionary<JobType, double> LostZoneJobMultipliers = new Dictionary<JobType, double>
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

    public static readonly IReadOnlyDictionary<int, int> RecoveryDelayBySicknessBand = new Dictionary<int, int>
    {
        [19] = 0,
        [29] = 1,
        [39] = 2,
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
    };

    public static int ComputeRecoveryDays(int sickness)
    {
        if (sickness <= 19)
            return BaseRecoveryTimeDays;

        if (sickness <= 29)
            return BaseRecoveryTimeDays + 1;

        if (sickness <= 39)
            return BaseRecoveryTimeDays + 2;

        return int.MaxValue;
    }

    public static int ClampStat(int value) => Math.Clamp(value, 0, 100);
}