using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class InspireThePeopleOrder : EmergencyOrderBase
{
    private const int MoraleGain = 15;
    private const int MaterialsCost = 15;

    public InspireThePeopleOrder() : base("inspire_people", "Inspire the People", $"+{MoraleGain} morale today, -{MaterialsCost} materials.")
    {
    }

    public override string GetDynamicTooltip(GameState state) => $"+{MoraleGain} morale today, -{MaterialsCost} materials.";

    public override bool CanIssue(GameState state, Zones.ZoneId? selectedZone, out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public override void Apply(GameState state, Zones.ZoneId? selectedZone, DayResolutionReport report)
    {
        StateChangeApplier.AddMorale(state, MoraleGain, report, ReasonTags.OrderEffect, Name);
        StateChangeApplier.AddResource(state, Resources.ResourceKind.Materials, -MaterialsCost, report, ReasonTags.OrderEffect, Name);
    }
}