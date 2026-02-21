using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class EmergencyWaterRationOrder : EmergencyOrderBase
{
    private const double WaterConsumptionMultiplier = 0.5;
    private const int SicknessHit = 10;

    public EmergencyWaterRationOrder() : base("emergency_water_ration", "Emergency Water Ration", "-50% water consumption today, +10 sickness.")
    {
    }

    public override bool CanIssue(GameState state, Zones.ZoneId? selectedZone, out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public override void Apply(GameState state, Zones.ZoneId? selectedZone, DayResolutionReport report)
    {
        state.DailyEffects.WaterConsumptionMultiplier *= WaterConsumptionMultiplier;
        StateChangeApplier.AddSickness(state, SicknessHit, report, ReasonTags.OrderEffect, Name);
    }
}