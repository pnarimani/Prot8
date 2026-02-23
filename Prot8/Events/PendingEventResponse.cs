namespace Prot8.Events;

public sealed class PendingEventResponse(ITriggeredEvent triggeredEvent, IReadOnlyList<EventResponse> responses)
{
    public ITriggeredEvent Event { get; } = triggeredEvent;
    public IReadOnlyList<EventResponse> Responses { get; } = responses;
}
