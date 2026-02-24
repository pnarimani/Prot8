using Prot8.Resources;
using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Events;

public sealed class SiegeBombardmentEvent : TriggeredEventBase
{
    private const int MinimumDay = 8;
    private const int TriggerChance = 20;
    private const int BaseDamage = 8;
    private const int DamagePerSiegeLevel = 2;
    private const int BaseFoodLost = 5;
    private const int FoodLostPerSiegeLevel = 3;
    private const int Deaths = 2;

    public SiegeBombardmentEvent() : base("siege_bombardment", "Siege Bombardment",
        $"After day {MinimumDay}, {TriggerChance}% daily chance. -{BaseDamage}+ integrity to random zone, -{BaseFoodLost}+ food (scales with siege), {Deaths} deaths.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        if (state.Day < MinimumDay)
        {
            return false;
        }

        return state.Random.Next(1, 101) <= TriggerChance;
    }

    public override void Apply(GameState state, ResolutionEntry entry)
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
            var damage = BaseDamage + state.SiegeIntensity * DamagePerSiegeLevel;
            target.Integrity -= damage;
            entry.Write($"{Name}: bombardment struck {target.Name} for -{damage} integrity.");

            if (target.Integrity <= 0)
            {
                state.LoseZone(target.Id, false, entry);
            }
        }

        var foodLost = BaseFoodLost + state.SiegeIntensity * FoodLostPerSiegeLevel;
        state.AddResource(ResourceKind.Food, -foodLost, entry);
        state.ApplyDeath(Deaths, entry);

        StartCooldown(state);
    }
}
