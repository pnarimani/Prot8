using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Orders;

public sealed class VoluntaryEvacuationOrder : EmergencyOrderBase
{
    public VoluntaryEvacuationOrder() : base("voluntary_evacuation", "Voluntary Evacuation", "Evacuate the active perimeter zone if eligible (irreversible). Consumes today's action.")
    {
    }

    public override bool CanIssue(GameState state, ZoneId? selectedZone, out string reason)
    {
        return ZoneRules.CanEvacuate(state, state.ActivePerimeterZone.Id, out reason);
    }

    public override void Apply(GameState state, ZoneId? selectedZone, DayResolutionReport report)
    {
        StateChangeApplier.LoseZone(state, state.ActivePerimeterZone.Id, true, report);
    }
}
