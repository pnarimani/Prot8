using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class TotalCollapseEvent : TriggeredEventBase
{
    public TotalCollapseEvent() : base("total_collapse", "Total Collapse",
        $"Triggers when food and water stay at zero for {GameBalance.FoodWaterLossThresholdDays} consecutive days. Immediate game over.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.ConsecutiveBothFoodWaterZeroDays >= GameBalance.FoodWaterLossThresholdDays;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        state.GameOver = true;
        state.GameOverCause = GameOverCause.TotalCollapse;
        state.GameOverDetails = "Food and water both reached zero for too long.";
        report.Add(ReasonTags.Event, $"{Name}: full supply collapse. Immediate game over.");
        StartCooldown(state);
    }
}