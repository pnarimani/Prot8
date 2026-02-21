using System.Collections.Generic;

namespace Prot8.Events;

public static class EventCatalog
{
    private static readonly IReadOnlyList<ITriggeredEvent> AllEvents = new ITriggeredEvent[]
    {
        new HungerRiotEvent(),
        new FeverOutbreakEvent(),
        new DesertionWaveEvent(),
        new WallBreachAttemptEvent(),
        new FireInArtisanQuarterEvent(),
        new CouncilRevoltEvent(),
        new TotalCollapseEvent()
    };

    public static IReadOnlyList<ITriggeredEvent> GetAll() => AllEvents;
}