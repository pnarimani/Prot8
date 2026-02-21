using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class DivertSuppliesToRepairsOrder : EmergencyOrderBase
{
    private const int FoodCost = 30;
    private const int WaterCost = 20;
    private const double RepairBoost = 1.5;

    public DivertSuppliesToRepairsOrder() : base("divert_supplies_repairs", "Divert Supplies to Repairs", "+50% repair output today, -30 food, -20 water.")
    {
    }

    public override bool CanIssue(GameState state, Zones.ZoneId? selectedZone, out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public override void Apply(GameState state, Zones.ZoneId? selectedZone, DayResolutionReport report)
    {
        state.DailyEffects.RepairOutputMultiplier *= RepairBoost;
        StateChangeApplier.AddResource(state, Resources.ResourceKind.Food, -FoodCost, report, ReasonTags.OrderEffect, Name);
        StateChangeApplier.AddResource(state, Resources.ResourceKind.Water, -WaterCost, report, ReasonTags.OrderEffect, Name);
    }
}