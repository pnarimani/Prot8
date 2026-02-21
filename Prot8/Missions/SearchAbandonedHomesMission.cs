using Prot8.Constants;
using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Missions;

public sealed class SearchAbandonedHomesMission : MissionBase
{
    public SearchAbandonedHomesMission() : base("search_abandoned_homes", "Search Abandoned Homes", "Can recover materials or medicine, but plague exposure is possible.", 2, GameBalance.MissionSearchHomesWorkers)
    {
    }

    public override void ResolveOutcome(GameState state, ActiveMission mission, DayResolutionReport report)
    {
        var roll = RollPercent(state);
        if (roll <= 50)
        {
            StateChangeApplier.AddResource(state, ResourceKind.Materials, 60, report, ReasonTags.Mission, Name);
            report.AddResolvedMission($"{Name}: recovered +60 materials.");
            return;
        }

        if (roll <= 80)
        {
            StateChangeApplier.AddResource(state, ResourceKind.Medicine, 40, report, ReasonTags.Mission, Name);
            report.AddResolvedMission($"{Name}: recovered +40 medicine.");
            return;
        }

        StateChangeApplier.AddSickness(state, 15, report, ReasonTags.Mission, Name);
        report.AddResolvedMission($"{Name}: plague exposure (+15 sickness).");
    }
}