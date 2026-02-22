using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class VoluntaryEvacuationOrder : IEmergencyOrder
{
    public string Id => "voluntary_evacuation";
    public string Name => "Voluntary Evacuation";

    public string GetTooltip(GameState state) => "Evacuate the active perimeter zone if eligible (irreversible).";

    public bool CanIssue(GameState state, out string reason) =>
        ZoneRules.CanEvacuate(state, state.ActivePerimeterZone.Id, out reason);

    public void Apply(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.LoseZone(state, state.ActivePerimeterZone.Id, true, report);
    }
}