using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class DivertSuppliesToRepairsOrder : IEmergencyOrder
{
    const int FoodCost = 30;
    const int WaterCost = 20;
    const double RepairBoost = 1.5;

    public string Id => "divert_supplies";
    public string Name => "Divert Supplies to Repairs";

    public string GetTooltip(GameState state) =>
        $"+{(RepairBoost - 1) * 100}% repair output today, -{FoodCost} food, -{WaterCost} water.";

    public bool CanIssue(GameState state, out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public void Apply(GameState state, DayResolutionReport report)
    {
        state.DailyEffects.RepairOutputMultiplier *= RepairBoost;
        StateChangeApplier.AddResource(state, ResourceKind.Food, -FoodCost, report, ReasonTags.OrderEffect, Name);
        StateChangeApplier.AddResource(state, ResourceKind.Water, -WaterCost, report, ReasonTags.OrderEffect, Name);
    }
}