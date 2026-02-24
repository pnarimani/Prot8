using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class CrackdownPatrolsOrder : IEmergencyOrder
{
    const int UnrestReduction = 25;
    const int Deaths = 3;
    const int MoraleHit = 15;
    const int UnrestThreshold = 40;

    public string Id => "crackdown_patrols";
    public string Name => "Crackdown Patrols";
    public int CooldownDays => 3;

    public string GetTooltip(GameState state) =>
        $"-{UnrestReduction} unrest, {Deaths} deaths, -{MoraleHit} morale. Requires unrest > {UnrestThreshold}.";

    public bool CanIssue(GameState state, out string reason)
    {
        if (state.Unrest <= UnrestThreshold)
        {
            reason = $"Requires unrest above {UnrestThreshold}.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void Apply(GameState state, ResolutionEntry entry)
    {
        entry.Write("Patrols sweep through the streets with steel. Dissent is crushed, but blood is spilled. The city is quiet — the quiet of fear.");
        state.AddUnrest(-UnrestReduction, entry);
        state.ApplyDeath(Deaths, entry);
        state.AddMorale(-MoraleHit, entry);
    }
}
