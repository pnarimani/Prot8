using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class BurnSurplusOrder : IEmergencyOrder
{
    private const int MaterialsCost = 5;
    private const int SicknessReduction = 3;
    private const int MoraleGain = 3;

    public string Id => "burn_surplus";
    public string Name => "Burn Surplus for Warmth";
    public int CooldownDays => 2;

    public string GetTooltip(GameState state) =>
        $"-{MaterialsCost} materials, -{SicknessReduction} sickness, +{MoraleGain} morale.";

    public bool CanIssue(GameState state, out string reason)
    {
        if (!state.Resources.Has(ResourceKind.Materials, MaterialsCost))
        {
            reason = $"Requires at least {MaterialsCost} materials.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void Apply(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddResource(state, ResourceKind.Materials, -MaterialsCost, report, ReasonTags.OrderEffect, Name);
        StateChangeApplier.AddSickness(state, -SicknessReduction, report, ReasonTags.OrderEffect, Name);
        StateChangeApplier.AddMorale(state, MoraleGain, report, ReasonTags.OrderEffect, Name);
    }
}
