using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class TotalCollapseEvent() : ITriggeredEvent
{
    public string Id => "total_collapse";
    public string Name => "Total Collapse";
    public string Description => "The city has run out of both food and water. There is nothing left to distribute and no way to sustain life. The end is no longer a threat, it has arrived.";

    public bool ShouldTrigger(GameState state)
    {
        return state.ConsecutiveBothFoodWaterZeroDays >= GameBalance.FoodWaterLossThresholdDays;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        state.GameOver = true;
        state.GameOverCause = GameOverCause.TotalCollapse;
        state.GameOverDetails = "Food and water both reached zero for too long.";
        entry.Write("The last rations are gone. The last barrels are dry. People collapse in the streets. You led them as far as you could, but this is where the siege ends.");
    }
}