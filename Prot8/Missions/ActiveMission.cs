namespace Prot8.Missions;

public sealed class ActiveMission(IMissionDefinition mission)
{
    public string MissionId => mission.Id; 

    public string MissionName => mission.Name;

    public int DaysRemaining { get; set; } = mission.DurationDays;

    public int WorkerCost => mission.WorkerCost;

    public int GuardCost => mission.GuardCost;
}