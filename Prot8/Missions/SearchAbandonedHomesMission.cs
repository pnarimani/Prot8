using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Missions;

public sealed class SearchAbandonedHomesMission : IMissionDefinition
{
    public string Id => "search_abandoned_homes";
    public string Name => "Search Abandoned Homes";
    public int DurationDays => 2;
    public int WorkerCost => 4;

    public string GetTooltip(GameState state)
    {
        return "+40 Materials, +5 Sickness (45%) | +25 Medicine, +5 Sickness (35%) | +15 Sickness, 2 Deaths (20%)";
    }

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
            StateChangeApplier.AddResource(state, ResourceKind.Materials, 40, report, ReasonTags.Mission, Name);
            StateChangeApplier.AddSickness(state, 5, report, ReasonTags.Mission, $"{Name} exposure");
            report.AddResolvedMission($"{Name}: recovered +40 materials (+5 sickness).");
            return;
        }

        if (roll <= 80)
        {
            StateChangeApplier.AddResource(state, ResourceKind.Medicine, 25, report, ReasonTags.Mission, Name);
            StateChangeApplier.AddSickness(state, 5, report, ReasonTags.Mission, $"{Name} exposure");
            report.AddResolvedMission($"{Name}: recovered +25 medicine (+5 sickness).");
            return;
        }

        StateChangeApplier.AddSickness(state, 15, report, ReasonTags.Mission, Name);
        StateChangeApplier.ApplyDeaths(state, 2, report, ReasonTags.Mission, $"{Name} plague");
        report.AddResolvedMission($"{Name}: plague exposure (+15 sickness, 2 deaths).");
    }
}
