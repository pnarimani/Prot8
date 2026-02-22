using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class CrackdownPatrolsOrder : EmergencyOrderBase
{
    private const int UnrestReduction = 20;
    private const int Deaths = 2;
    private const int MoraleHit = 10;

    public CrackdownPatrolsOrder() : base("crackdown_patrols", "Crackdown Patrols", $"-{UnrestReduction} unrest today, {Deaths} deaths, -{MoraleHit} morale.")
    {
    }

    public override string GetDynamicTooltip(GameState state) => $"-{UnrestReduction} unrest today, {Deaths} deaths, -{MoraleHit} morale.";

    public override bool CanIssue(GameState state, Zones.ZoneId? selectedZone, out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public override void Apply(GameState state, Zones.ZoneId? selectedZone, DayResolutionReport report)
    {
        StateChangeApplier.AddUnrest(state, -UnrestReduction, report, ReasonTags.OrderEffect, Name);
        StateChangeApplier.ApplyDeaths(state, Deaths, report, ReasonTags.OrderEffect, Name);
        StateChangeApplier.AddMorale(state, -MoraleHit, report, ReasonTags.OrderEffect, Name);
    }
}