using Prot8.Simulation;

namespace Prot8.Missions;

public sealed class NightRaidMission : IMissionDefinition
{
    const int GreatChance = 30;
    const int OkChance = 40;

    public string Id => "night_raid";
    public string Name => "Night Raid";
    public int DurationDays => 2;
    public int WorkerCost => 6;

    public string GetTooltip(GameState state) =>
        $"Siege Delay +3 days ({GreatChance}%) | Siege Delay +2 ({OkChance}%) | {WorkerCost} Deaths, +15 Unrest ({100 - GreatChance - OkChance}%)";

    public bool CanStart(GameState state, out string reason)
    {
        reason = "";
        return true;
    }

    public void ResolveOutcome(GameState state, ActiveMission mission, DayResolutionReport report)
    {
        var roll = state.RollPercent();
        if (roll <= GreatChance)
        {
            state.SiegeEscalationDelayDays += 3;
            report.AddResolvedMission($"{Name}: major disruption, siege escalation delayed by 3 days.");
            return;
        }

        if (roll <= GreatChance + OkChance)
        {
            state.SiegeEscalationDelayDays += 2;
            report.AddResolvedMission($"{Name}: partial success, siege escalation delayed by 2 days.");
            return;
        }

        StateChangeApplier.ApplyDeaths(state, WorkerCost, report, ReasonTags.Mission, Name);
        StateChangeApplier.AddUnrest(state, 15, report, ReasonTags.Mission, Name);
        report.AddResolvedMission($"{Name}: operation failed ({WorkerCost} deaths, +15 unrest).");
    }
}
