using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Missions;

public sealed class NegotiateBlackMarketeersMission : IMissionDefinition
{
    public string Id => "negotiate";
    public string Name => "Negotiate Black Marketeers";
    public int DurationDays => 3;
    public int WorkerCost => 2;
    public string GetTooltip(GameState state) => "+60 water, +10 unrest (45%) | +50 food, +10 unrest (30%) | +25 unrest, 2 deaths (25%)";

    public bool CanStart(GameState state, out string reason)
    {
        reason = "";
        return true;
    }

    public void ResolveOutcome(GameState state, ActiveMission mission, DayResolutionReport report)
    {
        var roll = state.RollPercent();
        if (roll <= 45)
        {
            StateChangeApplier.AddResource(state, ResourceKind.Water, 60, report, ReasonTags.Mission, Name);
            StateChangeApplier.AddUnrest(state, 10, report, ReasonTags.Mission, $"{Name} corruption");
            report.AddResolvedMission($"{Name}: acquired +60 water (+10 unrest).");
            return;
        }

        if (roll <= 75)
        {
            StateChangeApplier.AddResource(state, ResourceKind.Food, 50, report, ReasonTags.Mission, Name);
            StateChangeApplier.AddUnrest(state, 10, report, ReasonTags.Mission, $"{Name} corruption");
            report.AddResolvedMission($"{Name}: acquired +50 food (+10 unrest).");
            return;
        }

        StateChangeApplier.AddUnrest(state, 25, report, ReasonTags.Mission, Name);
        StateChangeApplier.ApplyDeaths(state, 2, report, ReasonTags.Mission, $"{Name} betrayal");
        report.AddResolvedMission($"{Name}: betrayal (+25 unrest, 2 deaths).");
    }
}
