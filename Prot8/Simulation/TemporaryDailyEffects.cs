namespace Prot8.Simulation;

public sealed class TemporaryDailyEffects
{
    public double FoodConsumptionMultiplier { get; set; } = 1.0;

    public double WaterConsumptionMultiplier { get; set; } = 1.0;

    public double ProductionMultiplier { get; set; } = 1.0;

    public double WaterProductionMultiplier { get; set; } = 1.0;

    public double FoodProductionMultiplier { get; set; } = 1.0;

    public double MaterialsProductionMultiplier { get; set; } = 1.0;

    public double RepairOutputMultiplier { get; set; } = 1.0;

    public double MissionSuccessBonus { get; set; }

    public double RepairProductionMultiplier { get; set; } = 1.0;

    public double MedicineUsageMultiplier { get; set; } = 1.0;

    public double FuelConsumptionMultiplier { get; set; } = 1.0;

    public bool DustStormActive { get; set; }

    public int QuarantineSicknessReduction { get; set; }

    public Zones.ZoneId? QuarantineZone { get; set; }
}