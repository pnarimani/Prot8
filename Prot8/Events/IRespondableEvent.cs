using Prot8.Simulation;

namespace Prot8.Events;

public interface IRespondableEvent
{
    IReadOnlyList<EventResponse> GetResponses(GameState state);
    void ApplyResponse(string responseId, GameState state, DayResolutionReport report);
}
