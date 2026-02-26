using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Missions;

public sealed class ScoutingMission : IMissionDefinition
{
    public string Id => "scouting";
    public string Name => "Scouting Mission";
    public int DurationDays => 2;
    public int WorkerCost => 2;

    public string GetTooltip(GameState state)
    {
        var successChance = GetSuccessChance(state);
        var failChance = 100 - successChance;
        return $"Intel buff {GameBalance.IntelBuffDurationDays} days + Siege Warning ({successChance}%) | {GameBalance.ScoutingFailDeaths} Deaths, +{GameBalance.ScoutingFailUnrest} Unrest ({failChance}%)";
    }

    public bool CanStart(GameState state, out string reason)
    {
        if (!GameBalance.EnableScoutingMission)
        {
            reason = "Scouting missions are not enabled.";
            return false;
        }

        reason = "";
        return true;
    }

    public void ResolveOutcome(GameState state, ActiveMission mission, ResolutionEntry entry)
    {
        state.Flags.Fortification.Add(1, lifetimeDays: 5);
        var successChance = GetSuccessChance(state);
        var roll = state.RollPercent();

        if (roll <= successChance)
        {
            state.IntelBuffDaysRemaining = GameBalance.IntelBuffDurationDays;
            state.IntelWarningPending = true;
            entry.Write(
                $"The scouts returned with valuable intelligence. Intel buff active for {GameBalance.IntelBuffDurationDays} days. Enemy movements mapped.");
            return;
        }

        state.ApplyDeath(GameBalance.ScoutingFailDeaths, entry);
        state.AddUnrest(GameBalance.ScoutingFailUnrest, entry);
        entry.Write(
            $"The scouting party was discovered. {GameBalance.ScoutingFailDeaths} scouts killed. No intel gathered.");
    }

    int GetSuccessChance(GameState state)
    {
        var chance = GameBalance.ScoutingSuccessChance;
        var bonus = (int)(state.DailyEffects.MissionSuccessBonus * 100);
        return Math.Min(95, chance + bonus);
    }
}
