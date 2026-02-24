using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class DilutedWaterLaw : ILaw
{
    private const double WaterConsumptionMultiplier = 0.75;
    private const int DailySickness = 3;
    private const int MoraleHit = 10;

    public string Id => "diluted_water";
    public string Name => "Diluted Water";
    public string GetTooltip(GameState state) => $"-{(1 - WaterConsumptionMultiplier) * 100}% water consumption, +{DailySickness} sickness/day, -{MoraleHit} morale on enact. Requires prior water deficit.";

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
        state.AddMorale(-MoraleHit, entry);
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        state.DailyEffects.WaterConsumptionMultiplier *= WaterConsumptionMultiplier;
        state.AddSickness(DailySickness, entry);
    }
}
