using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class WorkerTakesLifeEvent : ITriggeredEvent
{
    public string Id => "worker_takes_life";
    public string Name => "A Worker Takes Their Own Life";
    public string Description => "The weight of despair has become too much. A worker is found dead by their own hand. The city falls silent.";

    public bool ShouldTrigger(GameState state)
    {
        if (!GameBalance.EnableHumanityScore)
            return false;

        return state.Flags.Humanity < 15 && state.Morale < 30;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        state.ApplyDeath(1, entry);
        state.AddMorale(-5, entry);
        entry.Write("A body is found in the quiet hours. No enemy arrow, no illness — just the unbearable weight of everything. The city grieves in silence.");
    }
}
