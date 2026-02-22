using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Laws;

public sealed class EmergencySheltersLaw : LawBase
{
    private const int CapacityGain = 30;
    private const int DailySickness = 10;
    private const int UnrestHit = 10;

    public EmergencySheltersLaw() : base("emergency_shelters", "Emergency Shelters", $"+{CapacityGain} capacity in Inner District, +{DailySickness} sickness/day, +{UnrestHit} unrest on enact. Requires first zone loss.")
    {
    }

    public override string GetDynamicTooltip(GameState state) => $"+{CapacityGain} capacity in Inner District, +{DailySickness} sickness/day, +{UnrestHit} unrest on enact. Requires first zone loss.";

    public override bool CanEnact(GameState state, out string reason)
    {
        if (state.ZoneLossOccurred)
        {
            reason = string.Empty;
            return true;
        }

        reason = "Requires first zone loss.";
        return false;
    }

    public override void OnEnact(GameState state, DayResolutionReport report)
    {
        var inner = state.GetZone(ZoneId.InnerDistrict);
        inner.Capacity += CapacityGain;
        state.RebalanceHousing();
        report.Add(ReasonTags.LawEnact, $"{Name}: Inner District capacity +{CapacityGain}.");
        StateChangeApplier.AddUnrest(state, UnrestHit, report, ReasonTags.LawEnact, Name);
    }

    public override void ApplyDaily(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddSickness(state, DailySickness, report, ReasonTags.LawPassive, Name);
    }
}