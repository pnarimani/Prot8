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

    public bool CanEnact(GameState state, out string reason)
    {
        if (state.Flags.PeopleFirst)
        {
            reason = "The people's covenant forbids such atrocity.";
            return false;
        }

        if (state.Flags.Tyranny < 7)
        {
            reason = "Requires absolute tyrannical control.";
            return false;
        }

        if (!state.Flags.MartialState)
        {
            reason = "Requires Martial State.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        state.Flags.Tyranny.Add(2);
        state.Flags.FearLevel.Add(2);
        state.AddUnrest(-UnrestReduction, entry);
        state.AddMorale(-MoraleHit, entry);
        state.ApplyDeath(Deaths, entry);
        entry.Write("The purge begins at dawn. Lists are read aloud. Doors are broken down. By nightfall, the streets run red. No one speaks against the regime now.");
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
    }
}
