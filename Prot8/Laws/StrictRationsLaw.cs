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

    public bool CanEnact(GameState state)
    {
        return true;
    }

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        entry.Write("You order portions halved. The bowls are empty, but they will stretch our stores.");
        state.AddMorale(-MoraleHit, entry);
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        entry.Write("Ration carts distribute meagre portions. Hunger gnaws at everyone.");
        state.DailyEffects.FoodConsumptionMultiplier.Apply("Strict Rations", FoodConsumptionMultiplier);
        state.AddUnrest(DailyUnrest, entry);
        state.AddSickness(DailySickness, entry);
    }
}
