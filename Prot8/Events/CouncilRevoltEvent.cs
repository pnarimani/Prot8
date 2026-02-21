using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class CouncilRevoltEvent : TriggeredEventBase
{
    public CouncilRevoltEvent() : base("council_revolt", "Council Revolt", "Trigger when unrest exceeds 85.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Unrest > GameBalance.RevoltThreshold;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        state.GameOver = true;
        state.GameOverCause = GameOverCause.CouncilRevolt;
        state.GameOverDetails = "Council revolt overwhelmed command.";
        report.Add(ReasonTags.Event, $"{Name}: the council revolted. Immediate game over.");
    }
}