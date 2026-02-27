using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class PurgeTheDisloyalLaw : ILaw
{
    private const int UnrestReduction = 30;
    private const int MoraleHit = 15;
    private const int Deaths = 8;

    public string Id => "purge_disloyal";
    public string Name => "Purge the Disloyal";

    public string GetTooltip(GameState state) =>
        $"-{UnrestReduction} unrest, -{MoraleHit} morale, {Deaths} deaths. One-time. Requires Tyranny >= 7 and Martial State.";

    public bool CanEnact(GameState state)
    {
        if (state.Flags.PeopleFirst)
        {
            return false;
        }

        if (state.Flags.Tyranny < 7)
        {
            return false;
        }

        if (!state.Flags.MartialState)
        {
            return false;
        }

        return true;
    }

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        state.Flags.Tyranny.Add(2);
        state.Flags.FearLevel.Add(2);
        if (GameBalance.EnableHumanityScore) state.Flags.Humanity.Add(-15);
        state.AddUnrest(-UnrestReduction, entry);
        state.AddMorale(-MoraleHit, entry);
        state.ApplyDeath(Deaths, entry);
        entry.Write("The purge begins at dawn. Lists are read aloud. Doors are broken down. By nightfall, the streets run red. No one speaks against the regime now.");
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
    }
}
