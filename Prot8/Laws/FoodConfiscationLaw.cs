using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class FoodConfiscationLaw : ILaw
{
    private const int FoodGain = 50;
    private const int UnrestHit = 25;
    private const int MoraleHit = 15;
    private const int Deaths = 3;
    private const int FoodThreshold = 60;

    public string Id => "food_confiscation";
    public string Name => "Food Confiscation";
    public string GetTooltip(GameState state) => $"+{FoodGain} food, +{UnrestHit} unrest, -{MoraleHit} morale, {Deaths} deaths. Requires food < {FoodThreshold}.";

    public bool CanEnact(GameState state, out string reason)
    {
        if (state.Resources[Resources.ResourceKind.Food] < FoodThreshold)
        {
            reason = string.Empty;
            return true;
        }

        reason = $"Requires food below {FoodThreshold}.";
        return false;
    }

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        state.AddResource(Resources.ResourceKind.Food, FoodGain, entry);
        state.AddUnrest(UnrestHit, entry);
        state.AddMorale(-MoraleHit, entry);
        state.ApplyDeath(Deaths, entry);
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
    }
}
