using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Decrees;

public sealed class RallyTheGuardsDecree : IDecree
{
    private const int FoodCost = 3;
    private const int UnrestReduction = 5;
    private const int GuardThreshold = 5;

    public string Id => "rally_guards";
    public string Name => "Rally the Guards";

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

    public void Apply(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddResource(state, ResourceKind.Food, -FoodCost, report, ReasonTags.OrderEffect, Name);
        StateChangeApplier.AddUnrest(state, -UnrestReduction, report, ReasonTags.OrderEffect, Name);
    }
}
