using System.Collections.Generic;

namespace Prot8.Simulation;

public sealed class DayResolutionReport
{
    private readonly List<DeltaLogEntry> _entries = new();
    private readonly List<string> _triggeredEvents = new();
    private readonly List<string> _resolvedMissions = new();

    public DayResolutionReport(int day)
    {
        Day = day;
    }

    public int Day { get; }

    public IReadOnlyList<DeltaLogEntry> Entries => _entries;

    public IReadOnlyList<string> TriggeredEvents => _triggeredEvents;

    public IReadOnlyList<string> ResolvedMissions => _resolvedMissions;

    public int FoodConsumedToday { get; set; }

    public int WaterConsumedToday { get; set; }

    public bool FoodDeficitToday { get; set; }

    public bool WaterDeficitToday { get; set; }

    public bool FuelDeficitToday { get; set; }

    public int OvercrowdingStacksToday { get; set; }

    public int RecoveredWorkersToday { get; set; }

    public int RecoveryMedicineSpentToday { get; set; }

    public bool RecoveryEnabledToday { get; set; }

    public string? RecoveryBlockedReason { get; set; }

    public void Add(string tag, string message)
    {
        _entries.Add(new DeltaLogEntry(tag, message));
    }

    public void AddTriggeredEvent(string eventName)
    {
        _triggeredEvents.Add(eventName);
    }

    public void AddResolvedMission(string missionName)
    {
        _resolvedMissions.Add(missionName);
    }
}