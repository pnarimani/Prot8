using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class SoupKitchensOrder : IEmergencyOrder
{
    private const int FoodCost = 40;
    private const int UnrestReduction = 15;

    public string Id => "soup_kitchens";
    public string Name => "Soup Kitchens";
    public string GetTooltip(GameState state) => $"-{UnrestReduction} unrest today, -{FoodCost} food.";

    public bool CanIssue(GameState state, out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public void Apply(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddUnrest(state, -UnrestReduction, report, ReasonTags.OrderEffect, Name);
        StateChangeApplier.AddResource(state, Resources.ResourceKind.Food, -FoodCost, report, ReasonTags.OrderEffect, Name);
    }
}