using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class PublicExecutionsLaw : ILaw
{
    private const int UnrestReduction = 25;
    private const int MoraleHit = 20;
    private const int Deaths = 5;
    private const int UnrestThreshold = 60;

    public string Id => "public_executions";
    public string Name => "Public Executions";
    public string GetTooltip(GameState state) => $"-{UnrestReduction} unrest instantly, -{MoraleHit} morale, {Deaths} deaths. Requires unrest > {UnrestThreshold}.";

    public bool CanEnact(GameState state)
    {
        if (state.Flags.Faith >= 4)
        {
            return false;
        }

        if (state.Unrest > UnrestThreshold)
        {
            return true;
        }

        return false;
    }

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        state.AddUnrest(-UnrestReduction, entry);
        state.AddMorale(-MoraleHit, entry);
        state.ApplyDeath(Deaths, entry);
        state.Flags.Tyranny.Add(2);
        state.Flags.FearLevel.Add(2);
        state.Flags.IronFist.Set();
        if (GameBalance.EnableHumanityScore) state.Flags.Humanity.Add(-10);
        entry.Write("Five bodies hang from the gallows. The crowd watches in silence. Fear keeps the streets calm — for now.");
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
    }
}