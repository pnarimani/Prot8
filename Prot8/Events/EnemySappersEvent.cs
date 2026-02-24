using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class EnemySappersEvent : ITriggeredEvent
{
    public string Id => "enemy_sappers";
    public string Name => "Enemy Sappers";

    public string Description =>
        "The enemy has been tunneling beneath the walls. Their engineers work in silence, undermining the foundations stone by stone, waiting for the moment to strike.";

    const int TriggerDay = 14;
    const int IntegrityDamage = 5;
    const int SiegeIncrease = 1;

    public bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        entry.Write(
            "Enemy miners have been at work beneath the walls. Tunnels collapse in silence, and walls groan as their foundations are undermined.");
        foreach (var zone in state.Zones)
        {
            if (!zone.IsLost)
            {
                zone.Integrity -= IntegrityDamage;
                entry.Write($"{zone.Name} sustains damage from underground.");

                if (zone.Integrity <= 0)
                {
                    state.LoseZone(zone.Id, false, entry);
                }
            }
        }

        if (state.SiegeIntensity < GameBalance.MaxSiegeIntensity)
        {
            state.SiegeIntensity += SiegeIncrease;
            entry.Write("The siege tightens its grip. Enemy forces press harder.");
        }
    }
}