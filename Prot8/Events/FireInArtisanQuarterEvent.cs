using Prot8.Resources;
using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Events;

public sealed class FireInArtisanQuarterEvent : TriggeredEventBase
{
    private const int SiegeIntensityThreshold = 3;
    private const int TriggerChance = 15;
    private const int MaterialsLost = 40;
    private const int IntegrityDamage = 12;
    private const int Deaths = 1;

    public FireInArtisanQuarterEvent() : base("fire_artisan", "Fire in Artisan Quarter",
        $"At siege intensity {SiegeIntensityThreshold}+, {TriggerChance}% daily chance. -{MaterialsLost} materials, -{IntegrityDamage} Artisan Quarter integrity, {Deaths} death.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        if (state.IsZoneLost(Zones.ZoneId.ArtisanQuarter))
        {
            return false;
        }

        if (state.SiegeIntensity < SiegeIntensityThreshold)
        {
            return false;
        }

        return state.Random.Next(1, 101) <= TriggerChance;
    }

    public override void Apply(GameState state, ResolutionEntry entry)
    {
        state.AddResource(ResourceKind.Materials, -MaterialsLost, entry);
        var artisan = state.GetZone(ZoneId.ArtisanQuarter);
        if (!artisan.IsLost)
        {
            artisan.Integrity -= IntegrityDamage;
            entry.Write($"{Name}: Artisan Quarter integrity -{IntegrityDamage}.");
            if (artisan.Integrity <= 0)
            {
                state.LoseZone(ZoneId.ArtisanQuarter, false, entry);
            }
        }

        state.ApplyDeath(Deaths, entry);        StartCooldown(state);
    }
}
