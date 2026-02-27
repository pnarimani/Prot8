using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class ReliefArmyNarrativeEvent(string id, string name, int daysBeforeArrival, string narrative) : ITriggeredEvent
{
    public string Id => id;
    public string Name => name;
    public string Description => narrative;

    public bool ShouldTrigger(GameState state)
    {
        if (!GameBalance.EnableReliefArmy)
            return false;

        var effectiveArrival = state.ActualReliefDay - state.ReliefAcceleration;
        return state.Day == effectiveArrival - daysBeforeArrival;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        entry.Write(narrative);
    }
}
