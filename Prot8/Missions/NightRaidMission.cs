using Prot8.Simulation;

namespace Prot8.Missions;

public sealed class NightRaidMission : IMissionDefinition
{
    const int GreatChance = 30;
    const int OkChance = 40;
    const int GreatSiegeDelay = 3;
    const int OkSiegeDelay = 2;
    const int FailUnrest = 15;
    const int FailDeaths = 6;
    const int GuardBonus = 5;
    const int GuardThreshold = 10;

    public string Id => "night_raid";
    public string Name => "Night Raid";
    public int DurationDays => 2;
    public int WorkerCost => 6;

    public string GetTooltip(GameState state)
    {
        var (greatChance, okChance) = GetChances(state);
        return $"Siege Delay +{GreatSiegeDelay} days ({greatChance}%) | Siege Delay +{OkSiegeDelay} ({okChance}%) | {FailDeaths} Deaths, +{FailUnrest} Unrest ({100 - greatChance - okChance}%)";
    }

    public bool CanStart(GameState state, out string reason)
    {
        reason = "";
        return true;
    }

    public void ResolveOutcome(GameState state, ActiveMission mission, ResolutionEntry entry)
    {
        var (greatChance, okChance) = GetChances(state);
        var roll = state.RollPercent();
        if (roll <= greatChance)
        {
            state.SiegeEscalationDelayDays += GreatSiegeDelay;
            entry.Write($"{Name}: major disruption, siege escalation delayed by {GreatSiegeDelay} days.");
            return;
        }

        if (roll <= greatChance + okChance)
        {
            state.SiegeEscalationDelayDays += OkSiegeDelay;
            entry.Write($"{Name}: partial success, siege escalation delayed by {OkSiegeDelay} days.");
            return;
        }

        state.ApplyDeath(FailDeaths, entry);
        state.AddUnrest(FailUnrest, entry);
        entry.Write($"{Name}: operation failed ({FailDeaths} deaths, +{FailUnrest} unrest).");
    }

    (int greatChance, int okChance) GetChances(GameState state)
    {
        var great = GreatChance;
        var ok = OkChance;
        if (state.Population.Guards >= GuardThreshold)
        {
            great += GuardBonus;
        }

        return (great, ok);
    }
}
