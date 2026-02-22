using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class StrictRationsLaw : LawBase
{
    private const double FoodConsumptionMultiplier = 0.75;
    private const int MoraleHit = 10;
    private const int DailyUnrest = 5;

    public StrictRationsLaw() : base("strict_rations", "Strict Rations", $"-{(1 - FoodConsumptionMultiplier) * 100}% food consumption, -{MoraleHit} morale on enact, +{DailyUnrest} unrest/day.")
    {
    }

    public override string GetDynamicTooltip(GameState state) => $"-{(1 - FoodConsumptionMultiplier) * 100}% food consumption, -{MoraleHit} morale on enact, +{DailyUnrest} unrest/day.";

    public override bool CanEnact(GameState state, out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public override void OnEnact(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddMorale(state, -MoraleHit, report, ReasonTags.LawEnact, Name);
    }

    public override void ApplyDaily(GameState state, DayResolutionReport report)
    {
        state.DailyEffects.FoodConsumptionMultiplier *= FoodConsumptionMultiplier;
        StateChangeApplier.AddUnrest(state, DailyUnrest, report, ReasonTags.LawPassive, Name);
    }
}