using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class SoupKitchensOrder : EmergencyOrderBase
{
    private const int FoodCost = 40;
    private const int UnrestReduction = 15;

    public SoupKitchensOrder() : base("soup_kitchens", "Soup Kitchens", $"-{UnrestReduction} unrest today, -{FoodCost} food.")
    {
    }

    public override string GetDynamicTooltip(GameState state) => $"-{UnrestReduction} unrest today, -{FoodCost} food.";

    public override bool CanIssue(GameState state, Zones.ZoneId? selectedZone, out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public override void Apply(GameState state, Zones.ZoneId? selectedZone, DayResolutionReport report)
    {
        StateChangeApplier.AddUnrest(state, -UnrestReduction, report, ReasonTags.OrderEffect, Name);
        StateChangeApplier.AddResource(state, Resources.ResourceKind.Food, -FoodCost, report, ReasonTags.OrderEffect, Name);
    }
}