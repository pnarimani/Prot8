using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Events;

public sealed class SiegeBombardmentEvent : TriggeredEventBase
{
    public SiegeBombardmentEvent() : base("siege_bombardment", "Siege Bombardment",
        "After day 8, 20% daily chance. Damages a random non-lost zone and destroys resources.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        if (state.Day < 8)
        {
            return false;
        }

        return state.Random.Next(1, 101) <= 20;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        var nonLostZones = new List<ZoneState>();
        foreach (var zone in state.Zones)
        {
            if (!zone.IsLost && zone.Id != ZoneId.Keep)
            {
                nonLostZones.Add(zone);
            }
        }

        if (nonLostZones.Count > 0)
        {
            var target = nonLostZones[state.Random.Next(nonLostZones.Count)];
            var damage = 8 + state.SiegeIntensity * 2;
            target.Integrity -= damage;
            report.Add(ReasonTags.Event, $"{Name}: bombardment struck {target.Name} for -{damage} integrity.");

            if (target.Integrity <= 0)
            {
                StateChangeApplier.LoseZone(state, target.Id, false, report);
            }
        }

        var foodLost = 5 + state.SiegeIntensity * 3;
        StateChangeApplier.AddResource(state, Resources.ResourceKind.Food, -foodLost, report, ReasonTags.Event, $"{Name} supplies destroyed");
        StateChangeApplier.ApplyDeaths(state, 2, report, ReasonTags.Event, Name);

        StartCooldown(state);
    }
}
