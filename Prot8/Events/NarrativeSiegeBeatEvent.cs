using Prot8.Simulation;

namespace Prot8.Events;

public sealed class NarrativeSiegeBeatEvent(string id, string name, int triggerDay, string narrativeText)
    : ITriggeredEvent
{
    public string Id => id;
    public string Name => name;
    public string Description => narrativeText;

    public bool ShouldTrigger(GameState state)
    {
        return state.Day == triggerDay;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        entry.Write(narrativeText);
    }
}