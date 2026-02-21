using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Orders;

public sealed class VoluntaryEvacuationOrder : EmergencyOrderBase
{
    public VoluntaryEvacuationOrder() : base("voluntary_evacuation", "Voluntary Evacuation", "Evacuate one eligible zone (irreversible). Consumes today's action.", true)
    {
    }

    public override bool CanIssue(GameState state, ZoneId? selectedZone, out string reason)
    {
        if (!selectedZone.HasValue)
        {
            reason = "Select a zone to evacuate.";
            return false;
        }

        return ZoneRules.CanEvacuate(state, selectedZone.Value, out reason);
    }

    public override void Apply(GameState state, ZoneId? selectedZone, DayResolutionReport report)
    {
        if (!selectedZone.HasValue)
        {
            return;
        }

        StateChangeApplier.LoseZone(state, selectedZone.Value, true, report);
    }
}