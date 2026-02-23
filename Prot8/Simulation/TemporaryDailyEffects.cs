namespace Prot8.Simulation;

public sealed class TemporaryDailyEffects
{
    public double FoodConsumptionMultiplier { get; set; } = 1.0;

    public double WaterConsumptionMultiplier { get; set; } = 1.0;

    public double ProductionMultiplier { get; set; } = 1.0;

    public double RepairOutputMultiplier { get; set; } = 1.0;

    public double MedicineUsageMultiplier { get; set; } = 1.0;

    public int QuarantineSicknessReduction { get; set; }

    public Zones.ZoneId? QuarantineZone { get; set; }
}