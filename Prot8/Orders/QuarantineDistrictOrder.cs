using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Orders;

public sealed class QuarantineDistrictOrder : EmergencyOrderBase
{
    private const int SicknessReduction = 10;

    public QuarantineDistrictOrder() : base("quarantine_district", "Quarantine District", "-10 sickness spread today, -50% production in selected zone today.", true)
    {
    }

    public override bool CanIssue(GameState state, ZoneId? selectedZone, out string reason)
    {
        if (!selectedZone.HasValue)
        {
            reason = "Select a zone to quarantine.";
            return false;
        }

        var zone = state.GetZone(selectedZone.Value);
        if (zone.IsLost)
        {
            reason = "Cannot quarantine a lost zone.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public override void Apply(GameState state, ZoneId? selectedZone, DayResolutionReport report)
    {
        if (!selectedZone.HasValue)
        {
            return;
        }

        state.DailyEffects.QuarantineZone = selectedZone.Value;
        state.DailyEffects.QuarantineSicknessReduction += SicknessReduction;
        report.Add(ReasonTags.OrderEffect, $"{Name}: {selectedZone.Value} production halved today, sickness spread reduced by {SicknessReduction}.");
    }
}