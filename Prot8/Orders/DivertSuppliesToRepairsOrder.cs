using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class DivertSuppliesToRepairsOrder : IEmergencyOrder
{
    const int MaterialsCost = 20;
    const int FuelCost = 5;
    const double RepairBoost = 2.0;

    public string Id => "divert_supplies";
    public string Name => "Divert Supplies to Repairs";
    public int CooldownDays => 3;

    public string GetTooltip(GameState state) =>
        $"+{(RepairBoost - 1) * 100}% repair output today, -{MaterialsCost} materials, -{FuelCost} fuel.";

    public bool CanIssue(GameState state, out string reason)
    {
        if (!state.Resources.Has(ResourceKind.Materials, MaterialsCost))
        {
            reason = $"Requires at least {MaterialsCost} materials.";
            return false;
        }

        if (!state.Resources.Has(ResourceKind.Fuel, FuelCost))
        {
            reason = $"Requires at least {FuelCost} fuel.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void Apply(GameState state, ResolutionEntry entry)
    {
        state.DailyEffects.RepairOutputMultiplier *= RepairBoost;
        state.AddResource(ResourceKind.Materials, -MaterialsCost, entry);
        state.AddResource(ResourceKind.Fuel, -FuelCost, entry);
    }
}
