using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Events;

public sealed class FireInArtisanQuarterEvent : TriggeredEventBase
{
    public FireInArtisanQuarterEvent() : base("fire_artisan", "Fire in Artisan Quarter", "At siege intensity 4+, 10% random daily chance.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        if (state.SiegeIntensity < 4)
        {
            return false;
        }

        return state.Random.Next(1, 101) <= 10;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddResource(state, Resources.ResourceKind.Materials, -50, report, ReasonTags.Event, Name);
        var artisan = state.GetZone(ZoneId.ArtisanQuarter);
        if (!artisan.IsLost)
        {
            artisan.Integrity -= 10;
            report.Add(ReasonTags.Event, $"{Name}: Artisan Quarter integrity -10.");
            if (artisan.Integrity <= 0)
            {
                StateChangeApplier.LoseZone(state, ZoneId.ArtisanQuarter, false, report);
            }
        }

        StartCooldown(state);
    }
}