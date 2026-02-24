using Prot8.Events;

namespace Prot8.Simulation;

public sealed class TemporaryDailyEffects
{
    public List<ITriggeredEvent> TriggeredEvents { get; } = [];
    public TrackedMultiplier FoodConsumptionMultiplier { get; } = new();

    public TrackedMultiplier WaterConsumptionMultiplier { get; } = new();

    public TrackedMultiplier ProductionMultiplier { get; } = new();

    public TrackedMultiplier WaterProductionMultiplier { get; } = new();

    public TrackedMultiplier FoodProductionMultiplier { get; } = new();

    public TrackedMultiplier MaterialsProductionMultiplier { get; } = new();

    public TrackedMultiplier RepairOutputMultiplier { get; } = new();

    public double MissionSuccessBonus { get; set; }

    public TrackedMultiplier RepairProductionMultiplier { get; } = new();

    public TrackedMultiplier MedicineUsageMultiplier { get; } = new();

    public TrackedMultiplier FuelConsumptionMultiplier { get; } = new();

    public bool DustStormActive { get; set; }

    public int QuarantineSicknessReduction { get; set; }

    public Zones.ZoneId? QuarantineZone { get; set; }
}
