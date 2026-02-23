using Prot8.Resources;
using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Events;

public sealed class OpeningBombardmentEvent : TriggeredEventBase
{
    private const int TriggerDay = 1;
    private const int IntegrityDamage = 10;
    private const int FoodLost = 10;

    public OpeningBombardmentEvent() : base("opening_bombardment", "Opening Bombardment",
        "Day 1: Outer Farms loses 10 integrity, -10 food from burning stores.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        var farms = state.GetZone(ZoneId.OuterFarms);
        farms.Integrity -= IntegrityDamage;
        report.Add(ReasonTags.Event, $"{Name}: the siege begins. Outer Farms struck for -{IntegrityDamage} integrity.");

        StateChangeApplier.AddResource(state, ResourceKind.Food, -FoodLost, report, ReasonTags.Event, $"{Name} burning stores");

        if (farms.Integrity <= 0)
        {
            StateChangeApplier.LoseZone(state, farms.Id, false, report);
        }

        StartCooldown(state);
    }
}
