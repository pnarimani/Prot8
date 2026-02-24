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

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        entry.Write("You order evacuation of the perimiter");
        entry.Write("The perimiter gets smaller, and smaller perimiter is easier to defend.");
        state.SiegeDamageMultiplier *= SiegeDamageMultiplier;
        entry.Write($"Daily siege damage multiplier: {SiegeDamageMultiplier.ToPercent()}");
        state.LoseZone(ZoneId.OuterFarms, true, entry);
        state.AddUnrest(UnrestHit, entry);
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
    }
}