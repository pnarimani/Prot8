using Prot8.Resources;
using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Events;

public sealed class OpeningBombardmentEvent : ITriggeredEvent
{
    public string Id => "opening_bombardment";
    public string Name => "Opening Bombardment";

    public string Description =>
        """
        The siege has begun. Enemy catapults loose their first volley.
        stones arc through the dawn sky and crash into the outer districts with a sound like the end of the world.
        """;

    const int TriggerDay = 1;
    const int IntegrityDamage = 10;
    const int FoodLost = 10;

    public bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        var farms = state.GetZone(ZoneId.OuterFarms);
        farms.Integrity -= IntegrityDamage;
        entry.Write(
            "The first boulders crash into Outer Farms. Smoke rises from burning granaries as the enemy demonstrates their intent.");

        state.AddResource(ResourceKind.Food, -FoodLost, entry);

        if (farms.Integrity <= 0)
        {
            state.LoseZone(farms.Id, false, entry);
        }
    }
}