using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Laws;

public sealed class AbandonOuterRingLaw : ILaw
{
    private const int UnrestHit = 15;
    private const double SiegeDamageMultiplier = 0.8;
    private const int IntegrityThreshold = 40;

    public string Id => "abandon_outer_ring";
    public string Name => "Abandon Outer Ring";
    public string GetTooltip(GameState state) => $"Immediately lose Outer Farms, x{SiegeDamageMultiplier} daily siege damage (-{(1 - SiegeDamageMultiplier) * 100}%), +{UnrestHit} unrest. Requires Outer Farms integrity < {IntegrityThreshold}.";

    public bool CanEnact(GameState state, out string reason)
    {
        var outerFarms = state.GetZone(ZoneId.OuterFarms);
        if (outerFarms.IsLost)
        {
            reason = "Outer Farms already lost.";
            return false;
        }

        if (outerFarms.Integrity < IntegrityThreshold)
        {
            reason = string.Empty;
            return true;
        }

        reason = $"Requires Outer Farms integrity below {IntegrityThreshold}.";
        return false;
    }

    public void OnEnact(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.LoseZone(state, ZoneId.OuterFarms, true, report);
        state.SiegeDamageMultiplier *= SiegeDamageMultiplier;
        report.Add(ReasonTags.LawEnact, $"{Name}: daily siege damage multiplier x{SiegeDamageMultiplier:0.00}.");
        StateChangeApplier.AddUnrest(state, UnrestHit, report, ReasonTags.LawEnact, Name);
    }

    public void ApplyDaily(GameState state, DayResolutionReport report)
    {
    }
}