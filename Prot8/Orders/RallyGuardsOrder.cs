using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class RallyGuardsOrder : IEmergencyOrder
{
    private const int FoodCost = 10;
    private const int UnrestReduction = 15;
    private const int MoraleGain = 5;
    private const int GuardThreshold = 5;

    public string Id => "rally_guards";
    public string Name => "Rally the Guards";
    public int CooldownDays => 3;

    public string GetTooltip(GameState state) =>
        $"-{FoodCost} food, -{UnrestReduction} unrest, +{MoraleGain} morale. Requires guards >= {GuardThreshold}.";

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
        state.AddMorale(MoraleGain, entry);
        entry.Write("A feast is laid out for the garrison. Guards eat well and parade through the streets. The people feel safer — but the food stores are lighter.");
    }
}
