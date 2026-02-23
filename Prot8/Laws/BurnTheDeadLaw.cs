using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class BurnTheDeadLaw : ILaw
{
    private const int DailySicknessReduction = 2;
    private const int DailyMoraleHit = 3;
    private const int DailyFuelCost = 3;
    private const int MoraleHit = 10;
    private const int SicknessThreshold = 35;

    public string Id => "burn_the_dead";
    public string Name => "Burn the Dead";
    public string GetTooltip(GameState state) => $"-{DailySicknessReduction} sickness/day, -{DailyMoraleHit} morale/day, -{DailyFuelCost} fuel/day, -{MoraleHit} morale on enact. Requires sickness > {SicknessThreshold}.";

    public bool CanEnact(GameState state, out string reason)
    {
        if (state.Sickness > SicknessThreshold)
        {
            reason = string.Empty;
            return true;
        }

        reason = $"Requires sickness above {SicknessThreshold}.";
        return false;
    }

    public void OnEnact(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddMorale(state, -MoraleHit, report, ReasonTags.LawEnact, Name);
    }

    public void ApplyDaily(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddSickness(state, -DailySicknessReduction, report, ReasonTags.LawPassive, Name);
        StateChangeApplier.AddMorale(state, -DailyMoraleHit, report, ReasonTags.LawPassive, Name);
        StateChangeApplier.AddResource(state, Resources.ResourceKind.Fuel, -DailyFuelCost, report, ReasonTags.LawPassive, Name);
    }
}
