using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class CrackdownPatrolsOrder : IEmergencyOrder
{
    const int UnrestReduction = 20;
    const int Deaths = 2;
    const int MoraleHit = 10;

    public string Id => "crackdown_patrols";
    public string Name => "Crackdown Patrols";
    public int CooldownDays => 3;

    public string GetTooltip(GameState state) =>
        $"-{UnrestReduction} unrest today, {Deaths} deaths, -{MoraleHit} morale.";

    public bool CanIssue(GameState state, out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public void Apply(GameState state, ResolutionEntry entry)
    {
        entry.Write("Patrols sweep through the streets with steel. Dissent is crushed, but blood is spilled. The city is quiet, but it is the quiet of fear.");
        state.AddUnrest(-UnrestReduction, entry);
        state.ApplyDeath(Deaths, entry);
        state.AddMorale(-MoraleHit, entry);
    }
}