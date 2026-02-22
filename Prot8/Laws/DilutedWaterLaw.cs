using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class DilutedWaterLaw : ILaw
{
    private const double WaterConsumptionMultiplier = 0.8;
    private const int DailySickness = 5;
    private const int MoraleHit = 5;

    public string Id => "diluted_water";
    public string Name => "Diluted Water";
    public string GetTooltip(GameState state) => $"-{WaterConsumptionMultiplier * 100}% water consumption, +{DailySickness} sickness/day, -{MoraleHit} morale on enact. Requires prior water deficit.";

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

    public void OnEnact(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddMorale(state, -MoraleHit, report, ReasonTags.LawEnact, Name);
    }

    public void ApplyDaily(GameState state, DayResolutionReport report)
    {
        state.DailyEffects.WaterConsumptionMultiplier *= WaterConsumptionMultiplier;
        StateChangeApplier.AddSickness(state, DailySickness, report, ReasonTags.LawPassive, Name);
    }
}