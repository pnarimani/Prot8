using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class InspireThePeopleOrder : IEmergencyOrder
{
    private const int MoraleGain = 10;
    private const int MaterialsCost = 15;
    private const int SicknessHit = 5;

    public string Id => "inspire_people";
    public string Name => "Inspire the People";
    public string GetTooltip(GameState state) => $"+{MoraleGain} morale today, -{MaterialsCost} materials, +{SicknessHit} sickness from public gathering.";

    public bool CanIssue(GameState state, out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public void Apply(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddMorale(state, MoraleGain, report, ReasonTags.OrderEffect, Name);
        StateChangeApplier.AddResource(state, Resources.ResourceKind.Materials, -MaterialsCost, report, ReasonTags.OrderEffect, Name);
        StateChangeApplier.AddSickness(state, SicknessHit, report, ReasonTags.OrderEffect, $"{Name} gathering");
    }
}
