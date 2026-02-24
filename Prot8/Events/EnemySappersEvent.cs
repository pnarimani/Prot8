using Prot8.Simulation;

namespace Prot8.Events;

public sealed class EnemySappersEvent : TriggeredEventBase
{
    private const int TriggerDay = 16;
    private const int IntegrityDamage = 5;
    private const int SiegeIncrease = 1;

    public EnemySappersEvent() : base("enemy_sappers", "Enemy Sappers",
        "Day 16: All zones lose 5 integrity. Siege intensity +1.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay;
    }

    public override void Apply(GameState state, ResolutionEntry entry)
    {
        foreach (var zone in state.Zones)
        {
            if (!zone.IsLost)
            {
                zone.Integrity -= IntegrityDamage;
                entry.Write($"{Name}: {zone.Name} -{IntegrityDamage} integrity.");

                if (zone.Integrity <= 0)
                {
                    state.LoseZone(zone.Id, false, entry);
                }
            }
        }

        if (state.SiegeIntensity < Constants.GameBalance.MaxSiegeIntensity)
        {
            state.SiegeIntensity += SiegeIncrease;
            entry.Write($"{Name}: siege intensity increased to {state.SiegeIntensity}.");
        }

        StartCooldown(state);
    }
}
