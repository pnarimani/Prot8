using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Laws;

public sealed class AbandonOuterRingLaw : LawBase
{
    private const int UnrestHit = 15;
    private const double SiegeDamageMultiplier = 0.8;
    private const int IntegrityThreshold = 40;

    public AbandonOuterRingLaw() : base("abandon_outer_ring", "Abandon the Outer Ring", $"Immediately lose Outer Farms, -{SiegeDamageMultiplier * 100}% daily siege damage, +{UnrestHit} unrest. Requires Outer Farms integrity < {IntegrityThreshold}.")
    {
    }

    public override string GetDynamicTooltip(GameState state) => $"Immediately lose Outer Farms, -{SiegeDamageMultiplier * 100}% daily siege damage, +{UnrestHit} unrest. Requires Outer Farms integrity < {IntegrityThreshold}.";

    public override bool CanEnact(GameState state, out string reason)
    {
        var outerFarms = state.GetZone(ZoneId.OuterFarms);
        if (outerFarms.IsLost)
        {
            reason = "Outer Farms already lost.";
            return false;
        }

        if (outerFarms.Integrity < 40)
        {
            reason = string.Empty;
            return true;
        }

        reason = "Requires Outer Farms integrity below 40.";
        return false;
    }

    public override void OnEnact(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.LoseZone(state, ZoneId.OuterFarms, true, report);
        state.SiegeDamageMultiplier *= SiegeDamageMultiplier;
        report.Add(ReasonTags.LawEnact, $"{Name}: daily siege damage multiplier x{SiegeDamageMultiplier:0.00}.");
        StateChangeApplier.AddUnrest(state, UnrestHit, report, ReasonTags.LawEnact, Name);
    }
}