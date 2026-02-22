using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Missions;

public sealed class NightRaidMission() : MissionBase("night_raid", "Night Raid on Siege Camp",
    "Siege Delay +3 days (40%) | Siege Delay +2 (20%) | 8 Deaths, +15 Unrest (20%)", 1, GameBalance.MissionNightRaidWorkers)
{
    private const int GreatChance = 40;
    private const int OkChance = 40;

    public override void ResolveOutcome(GameState state, ActiveMission mission, DayResolutionReport report)
    {
        var roll = RollPercent(state);
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

        StateChangeApplier.ApplyDeaths(state, 8, report, ReasonTags.Mission, Name);
        StateChangeApplier.AddUnrest(state, 15, report, ReasonTags.Mission, Name);
        report.AddResolvedMission($"{Name}: operation failed (8 deaths, +15 unrest).");
    }
}