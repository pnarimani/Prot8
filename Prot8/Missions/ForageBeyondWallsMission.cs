using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Missions;

public sealed class ForageBeyondWallsMission() : IMissionDefinition
{
    public string Id => "forage_beyond_walls";
    public string Name => "Forage Beyond Walls";
    public int DurationDays => 5;
    public int WorkerCost => 5;

    public string GetTooltip(GameState state)
    {
        var highChance = state.SiegeIntensity >= 4 ? 49 : 60;
        var mediumChance = state.SiegeIntensity >= 4 ? 21 : 25;
        var deathChance = 100 - highChance - mediumChance;
        return $"+120 Food ({highChance}%) | +80 Food ({mediumChance}%) | 5 Deaths ({deathChance}%)";
    }

    public bool CanStart(GameState state, out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public void ResolveOutcome(GameState state, ActiveMission mission, DayResolutionReport report)
    {
        var highFoodChance = state.SiegeIntensity >= 4 ? 49 : 60;
        var mediumFoodChance = state.SiegeIntensity >= 4 ? 21 : 25;

        var roll = state.RollPercent();
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