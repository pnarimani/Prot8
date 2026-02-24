using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class DilutedWaterLaw : ILaw
{
    private const double WaterConsumptionMultiplier = 0.75;
    private const int DailySickness = 1;
    private const int DailyUnrest = 2;
    private const int MoraleHit = 10;

    public string Id => "diluted_water";
    public string Name => "Diluted Water";
    public string GetTooltip(GameState state) => $"-{(1 - WaterConsumptionMultiplier) * 100}% water consumption, +{DailySickness} sickness/day, +{DailyUnrest} unrest/day, -{MoraleHit} morale on enact. Requires prior water deficit.";

    public bool CanEnact(GameState state, out string reason)
    {
        if (state.ConsecutiveWaterDeficitDays > 0 || state.WaterDeficitYesterday)
        {
            reason = string.Empty;
            return true;
        }

        reason = "Requires at least one water deficit day.";
        return false;
    }

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        entry.Write("The wells are stretched thin. You order water be cut with river water — it quenches thirst but carries disease.");
        state.AddMorale(-MoraleHit, entry);
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        entry.Write("Diluted water flows from the pumps. Thirst is sated, but bellies rumble with sickness.");
        state.DailyEffects.WaterConsumptionMultiplier.Apply("Diluted Water", WaterConsumptionMultiplier);
        state.AddSickness(DailySickness, entry);
        state.AddUnrest(DailyUnrest, entry);
    }
}
