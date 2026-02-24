using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class EmergencyWaterRationOrder : IEmergencyOrder
{
    const double WaterConsumptionMultiplier = 0.5;
    const int SicknessHit = 10;

    public string Id => "water_ration";
    public string Name => "Emergency Water Ration";
    public int CooldownDays => 3;

    public string GetTooltip(GameState state) =>
        $"-{(1 - WaterConsumptionMultiplier) * 100}% water consumption today, +{SicknessHit} sickness.";

    public bool CanIssue(GameState state, out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public void Apply(GameState state, ResolutionEntry entry)
    {
        state.DailyEffects.WaterConsumptionMultiplier *= WaterConsumptionMultiplier;
        state.AddSickness(SicknessHit, entry);
    }
}