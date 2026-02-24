using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class SoupKitchensOrder : IEmergencyOrder
{
    private const int FoodCost = 30;
    private const int UnrestReduction = 10;
    private const int MoraleGain = 5;

    public string Id => "soup_kitchens";
    public string Name => "Soup Kitchens";
    public int CooldownDays => 3;
    public string GetTooltip(GameState state) => $"-{UnrestReduction} unrest, +{MoraleGain} morale, -{FoodCost} food.";

    public bool CanIssue(GameState state, out string reason)
    {
        if (!state.Resources.Has(Resources.ResourceKind.Food, FoodCost))
        {
            reason = $"Requires at least {FoodCost} food.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void Apply(GameState state, ResolutionEntry entry)
    {
        state.AddUnrest(-UnrestReduction, entry);
        state.AddMorale(MoraleGain, entry);
        state.AddResource(Resources.ResourceKind.Food, -FoodCost, entry);
        entry.Write("Soup kitchens open in the lower quarter. The hungry line up for bowls of warmth. Morale lifts, but the food stores deplete.");
    }
}
