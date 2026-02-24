using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class StrictRationsLaw : ILaw
{
    private const double FoodConsumptionMultiplier = 0.75;
    private const int MoraleHit = 10;
    private const int DailyUnrest = 3;
    private const int DailySickness = 1;

    public string Id => "strict_rations";
    public string Name => "Strict Rations";
    public string GetTooltip(GameState state) => $"-{(1 - FoodConsumptionMultiplier) * 100}% food consumption, -{MoraleHit} morale on enact, +{DailyUnrest} unrest/day, +{DailySickness} sickness/day.";

    public bool CanEnact(GameState state, out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        state.AddMorale(-MoraleHit, entry);
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        state.DailyEffects.FoodConsumptionMultiplier *= FoodConsumptionMultiplier;
        state.AddUnrest(DailyUnrest, entry);
        state.AddSickness(DailySickness, entry);
    }
}
