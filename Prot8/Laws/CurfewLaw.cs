using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class CurfewLaw : ILaw
{
    private const int DailyUnrestReduction = 5;
    private const double ProductionMultiplier = 0.85;
    private const int UnrestThreshold = 50;

    public string Id => "curfew";
    public string Name => "Curfew";
    public string GetTooltip(GameState state) => $"-{DailyUnrestReduction} unrest/day, -{(1 - ProductionMultiplier) * 100}% production. Requires unrest > {UnrestThreshold}. Incompatible with Martial Law.";

    public bool CanEnact(GameState state, out string reason)
    {
        if (state.ActiveLawIds.Contains("martial_law"))
        {
            reason = "Incompatible with Martial Law.";
            return false;
        }

        if (state.Unrest > UnrestThreshold)
        {
            reason = string.Empty;
            return true;
        }

        reason = $"Requires unrest above {UnrestThreshold}.";
        return false;
    }

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        state.Flags.Fortification.Add(1);
        entry.Write("No one may walk the streets after sunset. The city becomes a prison at dusk.");
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        entry.Write("The curfew holds. The streets are empty, but tension simmers behind closed doors.");
        state.AddUnrest(-DailyUnrestReduction, entry);
        state.DailyEffects.ProductionMultiplier.Apply("Curfew", ProductionMultiplier);
    }
}
