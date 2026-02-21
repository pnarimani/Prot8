using Prot8.Simulation;

namespace Prot8.Events;

public interface ITriggeredEvent
{
    string Id { get; }

    string Name { get; }

    string Description { get; }

    bool ShouldTrigger(GameState state);

    void Apply(GameState state, DayResolutionReport report);
}