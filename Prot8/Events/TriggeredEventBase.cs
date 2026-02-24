using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Events;

public abstract class TriggeredEventBase : ITriggeredEvent
{
    protected TriggeredEventBase(string id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    public string Id { get; }

    public string Name { get; }

    public string Description { get; }

    public abstract bool ShouldTrigger(GameState state);

    public abstract void Apply(GameState state, ResolutionEntry entry);

    public bool IsOnCooldown(GameState state)
    {
        return state.EventCooldowns.TryGetValue(Id, out var remaining) && remaining > 0;
    }

    protected void StartCooldown(GameState state)
    {
        if (GameBalance.EventCooldownDays.TryGetValue(Id, out var days) && days > 0)
        {
            state.EventCooldowns[Id] = days;
        }
    }
}