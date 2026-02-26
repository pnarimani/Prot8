using System.Collections.Generic;

namespace Prot8.Missions;

public static class MissionCatalog
{
    private static readonly IReadOnlyList<IMissionDefinition> AllMissions = new IMissionDefinition[]
    {
        new ForageBeyondWallsMission(),
        new NightRaidMission(),
        new SearchAbandonedHomesMission(),
        new NegotiateBlackMarketeersMission(),
        new SabotageEnemySuppliesMission(),
        new ScoutingMission(),
        new SortieMission()
    };

    public static IReadOnlyList<IMissionDefinition> GetAll() => AllMissions;

    public static IMissionDefinition? Find(string missionId)
    {
        foreach (var mission in AllMissions)
        {
            if (mission.Id == missionId)
            {
                return mission;
            }
        }

        return null;
    }
}