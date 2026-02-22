using Prot8.Constants;
using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Missions;

public sealed class NegotiateBlackMarketeersMission : MissionBase
{
    public NegotiateBlackMarketeersMission() : base("negotiate_black_marketeers", "Negotiate with Black Marketeers", "May secure water or food, or provoke political scandal (unrest).", 4, GameBalance.MissionBlackMarketeersWorkers)
    {
    }

    public override void ResolveOutcome(GameState state, ActiveMission mission, DayResolutionReport report)
    {
        var roll = RollPercent(state);
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