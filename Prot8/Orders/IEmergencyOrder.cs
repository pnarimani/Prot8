using Prot8.Simulation;

namespace Prot8.Orders;

public interface IEmergencyOrder
{
    string Id { get; }

    string Name { get; }

    string Summary { get; }

    string GetDynamicTooltip(GameState state);

    bool RequiresZoneSelection { get; }

    bool CanIssue(GameState state, Zones.ZoneId? selectedZone, out string reason);

    void Apply(GameState state, Zones.ZoneId? selectedZone, DayResolutionReport report);
}