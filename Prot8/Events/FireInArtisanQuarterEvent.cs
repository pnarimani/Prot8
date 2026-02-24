using Prot8.Resources;
using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Events;

public sealed class FireInArtisanQuarterEvent() : ITriggeredEvent
{
    public string Id => "fire_artisan_quarter";
    public string Name => "Fire in the Artisan Quarter";
    public string Description => "Enemy incendiary projectiles have sparked a blaze in the Artisan Quarter. Workshops burn. Smoke billows over the rooftops as workers scramble to contain the flames.";

    private const int SiegeIntensityThreshold = 3;
    private const int TriggerChance = 15;
    private const int MaterialsLost = 40;
    private const int IntegrityDamage = 12;
    private const int Deaths = 1;

    public bool ShouldTrigger(GameState state)
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

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        entry.Write("Flames leap from workshop to workshop. The smell of burning timber fills the city. Materials are consumed and one life is lost before the blaze is controlled.");
        state.AddResource(ResourceKind.Materials, -MaterialsLost, entry);
        var artisan = state.GetZone(ZoneId.ArtisanQuarter);
        if (!artisan.IsLost)
        {
            artisan.Integrity -= IntegrityDamage;
            entry.Write($"The Artisan Quarter sustains heavy damage from the fire.");
            if (artisan.Integrity <= 0)
            {
                state.LoseZone(ZoneId.ArtisanQuarter, false, entry);
            }
        }

        state.ApplyDeath(Deaths, entry);
    }
}
