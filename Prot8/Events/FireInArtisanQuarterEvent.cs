using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Events;

public sealed class FireInArtisanQuarterEvent : TriggeredEventBase
{
    public FireInArtisanQuarterEvent() : base("fire_artisan", "Fire in Artisan Quarter", "At siege intensity 3+, 15% random daily chance.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        if (state.SiegeIntensity < 3)
        {
            return false;
        }

        return state.Random.Next(1, 101) <= 15;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddResource(state, Resources.ResourceKind.Materials, -40, report, ReasonTags.Event, Name);
        var artisan = state.GetZone(ZoneId.ArtisanQuarter);
        if (!artisan.IsLost)
        {
            artisan.Integrity -= 12;
            report.Add(ReasonTags.Event, $"{Name}: Artisan Quarter integrity -12.");
            if (artisan.Integrity <= 0)
            {
                StateChangeApplier.LoseZone(state, ZoneId.ArtisanQuarter, false, report);
            }
        }

        StateChangeApplier.ApplyDeaths(state, 1, report, ReasonTags.Event, Name);
        StartCooldown(state);
    }
}
