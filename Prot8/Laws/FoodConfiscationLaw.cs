using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class FoodConfiscationLaw : ILaw
{
    private const int FoodGain = 100;
    private const int UnrestHit = 20;
    private const int MoraleHit = 20;
    private const int FoodThreshold = 100;

    public string Id => "food_confiscation";
    public string Name => "Food Confiscation";
    public string GetTooltip(GameState state) => $"+{FoodGain} food instantly, +{UnrestHit} unrest, -{MoraleHit} morale. Requires food < {FoodThreshold}.";

    public bool CanEnact(GameState state, out string reason)
    {
        if (state.Resources[Resources.ResourceKind.Food] < 100)
        {
            reason = string.Empty;
            return true;
        }

        reason = "Requires food below 100.";
        return false;
    }

    public void OnEnact(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddResource(state, Resources.ResourceKind.Food, FoodGain, report, ReasonTags.LawEnact, Name);
        StateChangeApplier.AddUnrest(state, UnrestHit, report, ReasonTags.LawEnact, Name);
        StateChangeApplier.AddMorale(state, -MoraleHit, report, ReasonTags.LawEnact, Name);
    }

    public void ApplyDaily(GameState state, DayResolutionReport report)
    {
        
    }
}