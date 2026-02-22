using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Missions;

public sealed class NightRaidMission : MissionBase
{
    private const int GreatChance = 40;
    private const int OkChance = 40;

    public NightRaidMission() : base("night_raid", "Night Raid on Siege Camp", "May delay siege escalation, or fail causing deaths and unrest.", 1, GameBalance.MissionNightRaidWorkers)
    {
    }

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