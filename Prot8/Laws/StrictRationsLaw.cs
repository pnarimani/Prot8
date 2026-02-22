using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class StrictRationsLaw : ILaw
{
    private const double FoodConsumptionMultiplier = 0.75;
    private const int MoraleHit = 10;
    private const int DailyUnrest = 5;

    public string Id => "strict_rations";
    public string Name => "Strict Rations";
    public string GetTooltip(GameState state) => $"-{(1 - FoodConsumptionMultiplier) * 100}% food consumption, -{MoraleHit} morale on enact, +{DailyUnrest} unrest/day.";

    public bool CanEnact(GameState state, out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public void OnEnact(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddMorale(state, -MoraleHit, report, ReasonTags.LawEnact, Name);
    }

    public void ApplyDaily(GameState state, DayResolutionReport report)
    {
        state.DailyEffects.FoodConsumptionMultiplier *= FoodConsumptionMultiplier;
        StateChangeApplier.AddUnrest(state, DailyUnrest, report, ReasonTags.LawPassive, Name);
    }
}