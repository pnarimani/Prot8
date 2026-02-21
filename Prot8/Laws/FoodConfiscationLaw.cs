using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class FoodConfiscationLaw : LawBase
{
    private const int FoodGain = 100;
    private const int UnrestHit = 20;
    private const int MoraleHit = 20;

    public FoodConfiscationLaw() : base("food_confiscation", "Food Confiscation", "+100 food instantly, +20 unrest, -20 morale. Requires food < 100.")
    {
    }

    public override bool CanEnact(GameState state, out string reason)
    {
        if (state.Resources[Resources.ResourceKind.Food] < 100)
        {
            reason = string.Empty;
            return true;
        }

        reason = "Requires food below 100.";
        return false;
    }

    public override void OnEnact(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddResource(state, Resources.ResourceKind.Food, FoodGain, report, ReasonTags.LawEnact, Name);
        StateChangeApplier.AddUnrest(state, UnrestHit, report, ReasonTags.LawEnact, Name);
        StateChangeApplier.AddMorale(state, -MoraleHit, report, ReasonTags.LawEnact, Name);
    }
}