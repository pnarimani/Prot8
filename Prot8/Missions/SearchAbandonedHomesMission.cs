using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Missions;

public sealed class SearchAbandonedHomesMission : IMissionDefinition
{
    public string Id => "search_abandoned_homes";
    public string Name => "Search Abandoned Homes";
    public int DurationDays => 1;
    public int WorkerCost => 5;

    public string GetTooltip(GameState state)
    {
        return "+60 Materials (50%) | +40 Medicine (30%) | +15 Sickness (20%)";
    }

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