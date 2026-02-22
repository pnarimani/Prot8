using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Missions;

public sealed class NegotiateBlackMarketeersMission : IMissionDefinition
{
    public string Id => "negotiate";
    public string Name => "Negotiate Black Marketeers";
    public int DurationDays => 3;
    public int WorkerCost => 2;
    public string GetTooltip(GameState state) => "+100 water (50%) | +80 food (30%) | +20 unrest (20%)";

    public bool CanStart(GameState state, out string reason)
    {
        reason = "";
        return true;
    }

    public void ResolveOutcome(GameState state, ActiveMission mission, DayResolutionReport report)
    {
        var roll = state.RollPercent();
        if (roll <= 50)
        {
            StateChangeApplier.AddResource(state, ResourceKind.Water, 100, report, ReasonTags.Mission, Name);
            report.AddResolvedMission($"{Name}: acquired +100 water.");
            return;
        }

        if (roll <= 80)
        {
            StateChangeApplier.AddResource(state, ResourceKind.Food, 80, report, ReasonTags.Mission, Name);
            report.AddResolvedMission($"{Name}: acquired +80 food.");
            return;
        }

        StateChangeApplier.AddUnrest(state, 20, report, ReasonTags.Mission, Name);
        report.AddResolvedMission($"{Name}: scandal erupted (+20 unrest).");
    }
}