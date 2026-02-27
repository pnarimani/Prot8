using Prot8.Constants;
using Prot8.Resources;
using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Events;

public sealed class SiegeBombardmentEvent() : ITriggeredEvent
{
    public string Id => "siege_bombardment";
    public string Name => "Siege Bombardment";
    public string Description => "The enemy catapults thunder through the night. Stones fall without warning — each impact shaking the city and the resolve of those sheltering within.";

    const int MinimumDay = 8;
    const int TriggerChance = 20;
    const int BaseDamage = 8;
    const int DamagePerSiegeLevel = 2;
    const int BaseFoodLost = 5;
    const int FoodLostPerSiegeLevel = 3;
    const int Deaths = 1;
    const int Wounded = 2;

    public bool ShouldTrigger(GameState state)
    {
        if (state.Day < MinimumDay)
        {
            return false;
        }

        return state.Random.Next(1, 101) <= TriggerChance;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
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
            entry.Write($"Catapult stones slam into {target.Name}. Buildings crumble under the bombardment.");

            if (target.Integrity <= 0)
            {
                state.LoseZone(target.Id, false, entry);
            }
        }

        var foodLost = BaseFoodLost + state.SiegeIntensity * FoodLostPerSiegeLevel;
        state.AddResource(ResourceKind.Food, -foodLost, entry);
        state.ApplyDeath(Deaths, entry);
        state.ApplyWounding(Wounded, entry);
        entry.Write("The bombardment claims lives and destroys supplies. The siege grinds on.");

    }
}