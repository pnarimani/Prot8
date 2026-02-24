using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class CouncilRevoltEvent : TriggeredEventBase
{
    public CouncilRevoltEvent() : base("council_revolt", "Council Revolt",
        $"Triggers when unrest > {GameBalance.RevoltThreshold}. Immediate game over.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Unrest > GameBalance.RevoltThreshold;
    }

    public override void Apply(GameState state, ResolutionEntry entry)
    {
        state.GameOver = true;
        state.GameOverCause = GameOverCause.CouncilRevolt;
        state.GameOverDetails = "Council revolt overwhelmed command.";
        entry.Write($"{Name}: the council revolted. Immediate game over.");
        StartCooldown(state);
    }
}