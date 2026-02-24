using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class RallyGuardsOrder : IEmergencyOrder
{
    private const int FoodCost = 5;
    private const int UnrestReduction = 12;
    private const int GuardThreshold = 5;

    public string Id => "rally_guards";
    public string Name => "Rally the Guards";
    public int CooldownDays => 2;

    public string GetTooltip(GameState state) =>
        $"-{FoodCost} food, -{UnrestReduction} unrest. Requires guards >= {GuardThreshold}.";

    public bool CanIssue(GameState state, out string reason)
    {
        if (state.Population.Guards < GuardThreshold)
        {
            reason = $"Requires at least {GuardThreshold} guards.";
            return false;
        }

        if (!state.Resources.Has(ResourceKind.Food, FoodCost))
        {
            reason = $"Requires at least {FoodCost} food.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void Apply(GameState state, ResolutionEntry entry)
    {
        state.AddResource(ResourceKind.Food, -FoodCost, entry);
        state.AddUnrest(-UnrestReduction, entry);
        entry.Write("A feast is laid out for the garrison. Guards eat well and raise their swords in salute. The streets feel safer tonight.");
    }
}
