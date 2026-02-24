using Prot8.Simulation;

namespace Prot8.Events;

public interface IRespondableEvent : ITriggeredEvent
{
    IReadOnlyList<EventResponse> GetResponses(GameState state);
    void ResolveWithResponse(string responseId, GameState state, ResolutionEntry entry);
}
