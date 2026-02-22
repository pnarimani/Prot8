using Prot8.Constants;
using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Missions;

public sealed class ForageBeyondWallsMission() : MissionBase("forage_beyond_walls", "Forage Beyond Walls",
    "Usually returns food, or can result in deaths from ambush.", 5, GameBalance.MissionForageWorkers)
{
    public override void ResolveOutcome(GameState state, ActiveMission mission, DayResolutionReport report)
    {
        var highFoodChance = state.SiegeIntensity >= 4 ? 49 : 60;
        var mediumFoodChance = state.SiegeIntensity >= 4 ? 21 : 25;

        var roll = RollPercent(state);
        if (roll <= highFoodChance)
        {
            StateChangeApplier.AddResource(state, ResourceKind.Food, 120, report, ReasonTags.Mission, Name);
            report.AddResolvedMission($"{Name}: great haul (+120 food).");
            return;
        }

        if (roll <= highFoodChance + mediumFoodChance)
        {
            StateChangeApplier.AddResource(state, ResourceKind.Food, 80, report, ReasonTags.Mission, Name);
            report.AddResolvedMission($"{Name}: modest haul (+80 food).");
            return;
        }

        StateChangeApplier.ApplyDeaths(state, 5, report, ReasonTags.Mission, Name);
        report.AddResolvedMission($"{Name}: crew ambushed (5 deaths).");
    }
}