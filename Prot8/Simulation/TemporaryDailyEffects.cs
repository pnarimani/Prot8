namespace Prot8.Simulation;

public sealed class TemporaryDailyEffects
{
    public double FoodConsumptionMultiplier { get; set; } = 1.0;

    public double WaterConsumptionMultiplier { get; set; } = 1.0;

    public double ProductionMultiplier { get; set; } = 1.0;

    public double RepairOutputMultiplier { get; set; } = 1.0;

    public double MedicineUsageMultiplier { get; set; } = 1.0;

    public int FlatSicknessDelta { get; set; }

    public int FlatUnrestDelta { get; set; }

    public int FlatMoraleDelta { get; set; }

    public int FlatFoodDelta { get; set; }

    public int FlatWaterDelta { get; set; }

    public int FlatMaterialsDelta { get; set; }

    public int FlatFuelDelta { get; set; }

    public int FlatDeaths { get; set; }

    public int QuarantineSicknessReduction { get; set; }

    public Zones.ZoneId? QuarantineZone { get; set; }
}