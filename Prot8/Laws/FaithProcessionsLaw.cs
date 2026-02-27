using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class FaithProcessionsLaw : ILaw
{
    private const int MoraleGain = 15;
    private const int MaterialsCost = 10;
    private const int UnrestHit = 5;
    private const int DailySickness = 1;
    private const int DailyMorale = 2;
    private const int MoraleThreshold = 40;

    public string Id => "faith_processions";
    public string Name => "Faith Processions";
    public string GetTooltip(GameState state) => $"+{MoraleGain} morale on enact, -{MaterialsCost} materials, +{UnrestHit} unrest. Daily: +{DailyMorale} morale, +{DailySickness} sickness from gatherings. Requires morale < {MoraleThreshold}.";

    public bool CanEnact(GameState state)
    {
        if (state.Flags.IronFist)
        {
            return false;
        }

        if (state.Morale < MoraleThreshold)
        {
            return true;
        }

        return false;
    }

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        state.Flags.Faith.Add(2);
        state.Flags.FaithRisen.Set();
        if (GameBalance.EnableHumanityScore) state.Flags.Humanity.Add(5);
        state.AddMorale(MoraleGain, entry);
        state.AddResource(Resources.ResourceKind.Materials, -MaterialsCost, entry);
        state.AddUnrest(UnrestHit, entry);
        entry.Write("Priests lead processions through the streets, chanting prayers for salvation. The faithful find comfort; others see superstition.");
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        state.AddMorale(DailyMorale, entry);
        state.AddSickness(DailySickness, entry);
        entry.Write("Crowds gather at the temple. Faith spreads, but so does disease in the cramped pews.");
    }
}
