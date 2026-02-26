using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class FoodConfiscationLaw : ILaw
{
    private const int FoodGain = 35;
    private const int DailyUnrest = 2;
    private const int UnrestHit = 25;
    private const int MoraleHit = 15;
    private const int Deaths = 3;
    private const int FoodThreshold = 60;

    public string Id => "food_confiscation";
    public string Name => "Food Confiscation";
    public string GetTooltip(GameState state) => $"+{FoodGain} food, +{UnrestHit} unrest, -{MoraleHit} morale, {Deaths} deaths, +{DailyUnrest} unrest/day. Requires food < {FoodThreshold}.";

    public bool CanEnact(GameState state)
    {
        if (state.Flags.Faith >= 3)
        {
            return false;
        }

        if (state.Resources[Resources.ResourceKind.Food] < FoodThreshold)
        {
            return true;
        }

        return false;
    }

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        state.Flags.Tyranny.Add(1);
        state.AddResource(Resources.ResourceKind.Food, FoodGain, entry);
        state.AddUnrest(UnrestHit, entry);
        state.AddMorale(-MoraleHit, entry);
        state.ApplyDeath(Deaths, entry);
        entry.Write("Soldiers empty pantries and storerooms. Those who resist are made examples of. The food will keep the many alive — at the cost of the few.");
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        state.AddUnrest(DailyUnrest, entry);
        entry.Write("Resentment lingers from the confiscation. The people do not forget.");
    }
}
