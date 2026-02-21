namespace Prot8.Missions;

public sealed class ActiveMission
{
    public ActiveMission(string missionId, string missionName, int daysRemaining, int workerCost)
    {
        MissionId = missionId;
        MissionName = missionName;
        DaysRemaining = daysRemaining;
        WorkerCost = workerCost;
    }

    public string MissionId { get; }

    public string MissionName { get; }

    public int DaysRemaining { get; set; }

    public int WorkerCost { get; }
}