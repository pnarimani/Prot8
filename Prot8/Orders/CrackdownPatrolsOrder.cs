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

    public void Apply(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddUnrest(state, -UnrestReduction, report, ReasonTags.OrderEffect, Name);
        StateChangeApplier.ApplyDeaths(state, Deaths, report, ReasonTags.OrderEffect, Name);
        StateChangeApplier.AddMorale(state, -MoraleHit, report, ReasonTags.OrderEffect, Name);
    }
}