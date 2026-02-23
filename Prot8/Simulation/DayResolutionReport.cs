using Prot8.Events;

namespace Prot8.Simulation;

public sealed class DayResolutionReport(int day)
{
    public int Day { get; } = day;

    public List<DeltaLogEntry> Entries { get; } = [];

    public List<string> TriggeredEvents { get; } = [];

    public List<string> ResolvedMissions { get; } = [];

    public List<PendingEventResponse> PendingResponses { get; } = [];

    public List<EventResponseChoice> EventResponsesMade { get; } = [];

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

    public int StartFood { get; set; }
    public int StartWater { get; set; }
    public int StartFuel { get; set; }
    public int StartMorale { get; set; }
    public int StartUnrest { get; set; }
    public int StartSickness { get; set; }
    public int StartHealthyWorkers { get; set; }

    public void Add(string tag, string message)
    {
        Entries.Add(new DeltaLogEntry(tag, message));
    }

    public void AddTriggeredEvent(string eventName)
    {
        TriggeredEvents.Add(eventName);
    }

    public void AddResolvedMission(string missionName)
    {
        ResolvedMissions.Add(missionName);
    }
}