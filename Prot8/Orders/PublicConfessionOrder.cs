using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class PublicConfessionOrder : IEmergencyOrder
{
    private const int UnrestReduction = 20;
    private const int MoraleHit = 10;
    private const int Deaths = 2;

    public string Id => "public_confession";
    public string Name => "Public Confession";
    public int CooldownDays => 3;

    public string GetTooltip(GameState state) =>
        $"-{UnrestReduction} unrest, -{MoraleHit} morale, {Deaths} deaths. Requires Tyranny >= 4 and Fear Level >= 2.";

    public bool CanIssue(GameState state)
    {
        if (state.Flags.FaithRisen)
        {
            return false;
        }

        if (state.Flags.Tyranny < 4)
        {
            return false;
        }

        if (state.Flags.FearLevel < 2)
        {
            return false;
        }

        return true;
    }

    public void Apply(GameState state, ResolutionEntry entry)
    {
        state.Flags.Tyranny.Add(1);
        state.Flags.FearLevel.Add(1);
        state.AddUnrest(-UnrestReduction, entry);
        state.AddMorale(-MoraleHit, entry);
        state.ApplyDeath(Deaths, entry);
        entry.Write("Accused dissenters are dragged before the crowd and forced to confess their crimes. Two are made examples of. The rest kneel in silence.");
    }
}
