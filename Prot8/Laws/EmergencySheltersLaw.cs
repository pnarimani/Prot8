using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Laws;

public sealed class EmergencySheltersLaw : ILaw
{
    private const int CapacityGainPerZone = 4;
    private const int DailySickness = 2;
    private const int DailyUnrest = 2;
    private const int UnrestHit = 8;

    public string Id => "emergency_shelters";
    public string Name => "Emergency Shelters";
    public string GetTooltip(GameState state) => $"+{CapacityGainPerZone} capacity to all non-lost zones, +{DailySickness} sickness/day, +{DailyUnrest} unrest/day, +{UnrestHit} unrest on enact. Requires first zone loss.";

    public bool CanEnact(GameState state)
    {
        if (state.Flags.MartialState)
        {
            return false;
        }

        if (state.ZoneLossOccurred)
        {
            return true;
        }

        return false;
    }

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        state.Flags.Faith.Add(2);
        state.Flags.PeopleFirst.Set();
        foreach (var zone in state.Zones)
        {
            if (!zone.IsLost)
            {
                zone.Capacity += CapacityGainPerZone;
                entry.Write($"Emergency shelters erected in {zone.Name}. Cramped but shelter nonetheless.");
            }
        }
        entry.Write("The displaced huddle together. Desperation breeds disease.");
        state.AddUnrest(UnrestHit, entry);
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        entry.Write("Overcrowded shelters breed sickness. The coughing never stops.");
        state.AddSickness(DailySickness, entry);
        state.AddUnrest(DailyUnrest, entry);
    }
}
