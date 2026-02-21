using Prot8.Simulation;

namespace Prot8.Orders;

public abstract class EmergencyOrderBase : IEmergencyOrder
{
    protected EmergencyOrderBase(string id, string name, string summary, bool requiresZoneSelection = false)
    {
        Id = id;
        Name = name;
        Summary = summary;
        RequiresZoneSelection = requiresZoneSelection;
    }

    public string Id { get; }

    public string Name { get; }

    public string Summary { get; }

    public bool RequiresZoneSelection { get; }

    public abstract bool CanIssue(GameState state, Zones.ZoneId? selectedZone, out string reason);

    public abstract void Apply(GameState state, Zones.ZoneId? selectedZone, DayResolutionReport report);
}