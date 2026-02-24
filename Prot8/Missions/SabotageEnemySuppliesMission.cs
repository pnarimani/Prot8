using Prot8.Simulation;

namespace Prot8.Missions;

public sealed class SabotageEnemySuppliesMission : IMissionDefinition
{
    const int SuccessChance = 40;
    const int PartialChance = 30;
    const double SuccessDamageReduction = 0.7;
    const int SuccessDuration = 5;
    const double PartialDamageReduction = 0.85;
    const int PartialDuration = 3;
    const int FailDeaths = 4;
    const int FailUnrest = 20;

    public string Id => "sabotage_enemy";
    public string Name => "Sabotage Enemy Supplies";
    public int DurationDays => 3;
    public int WorkerCost => 4;

    public string GetTooltip(GameState state) =>
        $"Siege damage x{SuccessDamageReduction} for {SuccessDuration}d ({SuccessChance}%) | x{PartialDamageReduction} for {PartialDuration}d ({PartialChance}%) | {FailDeaths} Deaths, +{FailUnrest} Unrest ({100 - SuccessChance - PartialChance}%)";

    public bool CanStart(GameState state, out string reason)
    {
        reason = "";
        return true;
    }

    public void ResolveOutcome(GameState state, ActiveMission mission, ResolutionEntry entry)
    {
        var roll = state.RollPercent();
        if (roll <= SuccessChance)
        {
            state.SiegeDamageMultiplier = SuccessDamageReduction;
            state.SiegeDamageReductionDaysRemaining = SuccessDuration;
            entry.Write($"{Name}: major success! Siege damage reduced by 30% for {SuccessDuration} days.");
            return;
        }

        if (roll <= SuccessChance + PartialChance)
        {
            state.SiegeDamageMultiplier = PartialDamageReduction;
            state.SiegeDamageReductionDaysRemaining = PartialDuration;
            entry.Write($"{Name}: partial success. Siege damage reduced by 15% for {PartialDuration} days.");
            return;
        }

        state.ApplyDeath(FailDeaths, entry);
        state.AddUnrest(FailUnrest, entry);
        entry.Write($"{Name}: catastrophic failure ({FailDeaths} deaths, +{FailUnrest} unrest).");
    }
}
