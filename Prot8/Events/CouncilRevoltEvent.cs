using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class CouncilRevoltEvent() : ITriggeredEvent
{
    public string Id => "council_revolt";
    public string Name => "Council Revolt";
    public string Description => "The city council, long discontented with your rule, has armed their retainers and moved against you. The halls of power are theirs now.";

    public bool ShouldTrigger(GameState state)
    {
        return state.Unrest > GameBalance.RevoltThreshold;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        state.GameOver = true;
        state.GameOverCause = GameOverCause.CouncilRevolt;
        state.GameOverDetails = "Council revolt overwhelmed command.";
        entry.Write("Your reign ends in bloodshed. The council has taken over.");
    }
}