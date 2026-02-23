using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class DistributeFuelOrder : IEmergencyOrder
{
    private const int FuelCost = 5;
    private const int MoraleGain = 3;
    private const int SicknessReduction = 2;

    public string Id => "distribute_fuel";
    public string Name => "Distribute Fuel Reserves";
    public int CooldownDays => 2;

    public string GetTooltip(GameState state) =>
        $"-{FuelCost} fuel, +{MoraleGain} morale, -{SicknessReduction} sickness.";

    public bool CanIssue(GameState state, out string reason)
    {
        if (!state.Resources.Has(ResourceKind.Fuel, FuelCost))
        {
            reason = $"Requires at least {FuelCost} fuel.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void Apply(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddResource(state, ResourceKind.Fuel, -FuelCost, report, ReasonTags.OrderEffect, Name);
        StateChangeApplier.AddMorale(state, MoraleGain, report, ReasonTags.OrderEffect, Name);
        StateChangeApplier.AddSickness(state, -SicknessReduction, report, ReasonTags.OrderEffect, Name);
    }
}
