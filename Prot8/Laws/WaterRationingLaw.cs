using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class WaterRationingLaw : ILaw
{
    private const double WaterConsumptionMultiplier = 0.75;
    private const int DailySickness = 1;
    private const int DailyUnrest = 2;
    private const int MoraleHit = 10;

    public string Id => "water_rationing";
    public string Name => "Water Rationing";
    public string GetTooltip(GameState state) => $"-{(1 - WaterConsumptionMultiplier) * 100}% water consumption, +{DailySickness} sickness/day, +{DailyUnrest} unrest/day, -{MoraleHit} morale on enact.";

    public bool CanEnact(GameState state)
    {
        return true;
    }

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        entry.Write("You order water rations cut. The people drink less, but what remains is stretched with river water — it carries disease.");
        state.AddMorale(-MoraleHit, entry);
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        entry.Write("Water rations are thin. Thirst is barely sated, and the murky supply breeds sickness.");
        state.DailyEffects.WaterConsumptionMultiplier.Apply("Water Rationing", WaterConsumptionMultiplier);
        state.AddSickness(DailySickness, entry);
        state.AddUnrest(DailyUnrest, entry);
    }
}
