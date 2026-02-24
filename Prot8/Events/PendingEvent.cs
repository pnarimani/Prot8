namespace Prot8.Events;

public sealed class PendingEvent(ITriggeredEvent triggeredEvent, IReadOnlyList<EventResponse>? responses = null)
{
    public ITriggeredEvent Event { get; } = triggeredEvent;
    public IReadOnlyList<EventResponse>? Responses { get; } = responses;
}