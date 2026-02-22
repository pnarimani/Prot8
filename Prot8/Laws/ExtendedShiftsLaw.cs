using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class ExtendedShiftsLaw : ILaw
{
    private const double ProductionMultiplier = 1.25;
    private const int DailySickness = 8;
    private const int MoraleHit = 15;
    private const int MinimumDay = 5;

    public string Id => "extended_shifts";
    public string Name => "Extended Shifts";
    public string GetTooltip(GameState state) => $"+{(ProductionMultiplier - 1) * 100}% production, +{DailySickness} sickness/day, -{MoraleHit} morale on enact. Day {MinimumDay}+.";

    public bool CanEnact(GameState state, out string reason)
    {
        if (state.Day >= 5)
        {
            reason = string.Empty;
            return true;
        }

        reason = "Available from Day 5.";
        return false;
    }

    public void OnEnact(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddMorale(state, -MoraleHit, report, ReasonTags.LawEnact, Name);
    }

    public void ApplyDaily(GameState state, DayResolutionReport report)
    {
        state.DailyEffects.ProductionMultiplier *= ProductionMultiplier;
        StateChangeApplier.AddSickness(state, DailySickness, report, ReasonTags.LawPassive, Name);
    }
}