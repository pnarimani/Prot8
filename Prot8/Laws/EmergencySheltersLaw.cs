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

    public bool CanEnact(GameState state, out string reason)
    {
        if (state.ZoneLossOccurred)
        {
            reason = string.Empty;
            return true;
        }

        reason = "Requires first zone loss.";
        return false;
    }

    public void OnEnact(GameState state, DayResolutionReport report)
    {
        foreach (var zone in state.Zones)
        {
            if (!zone.IsLost)
            {
                zone.Capacity += CapacityGainPerZone;
                report.Add(ReasonTags.LawEnact, $"{Name}: {zone.Name} capacity +{CapacityGainPerZone}.");
            }
        }
        StateChangeApplier.AddUnrest(state, UnrestHit, report, ReasonTags.LawEnact, Name);
    }

    public void ApplyDaily(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddSickness(state, DailySickness, report, ReasonTags.LawPassive, Name);
        StateChangeApplier.AddUnrest(state, DailyUnrest, report, ReasonTags.LawPassive, Name);
    }
}
