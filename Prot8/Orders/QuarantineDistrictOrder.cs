using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Orders;

public sealed class QuarantineDistrictOrder : IEmergencyOrder
{
    private const int SicknessReduction = 8;
    private const int SicknessThreshold = 30;

    public string Id => "quarantine_district";
    public string Name => "Quarantine District";

    public string GetTooltip(GameState state) =>
        $"Quarantine the active perimeter zone: -50% production in that zone today, -{SicknessReduction} sickness. Requires sickness > {SicknessThreshold}.";

    public bool CanIssue(GameState state, out string reason)
    {
        if (state.Sickness <= SicknessThreshold)
        {
            reason = $"Requires sickness above {SicknessThreshold}.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void Apply(GameState state, DayResolutionReport report)
    {
        var zone = state.ActivePerimeterZone;
        state.DailyEffects.QuarantineZone = zone.Id;
        state.DailyEffects.QuarantineSicknessReduction = SicknessReduction;
        report.Add(ReasonTags.OrderEffect, $"{Name}: {zone.Name} quarantined. Production halved, sickness reduced.");
    }
}
