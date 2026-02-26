using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Missions;

public sealed class SortieMission : IMissionDefinition
{
    public string Id => "sortie";
    public string Name => "Sortie";
    public int DurationDays => 1;
    public int WorkerCost => 0;
    public int GuardCost => GameBalance.SortieGuardCost;

    public string GetTooltip(GameState state)
    {
        var failChance = 100 - GameBalance.SortieSuccessChance - GameBalance.SortiePartialChance;
        return $"Siege -1, +{GameBalance.SortieSuccessEscalationDelay}d delay ({GameBalance.SortieSuccessChance}%) | Damage x{GameBalance.SortiePartialDamageMultiplier} for {GameBalance.SortiePartialDurationDays}d ({GameBalance.SortiePartialChance}%) | {GameBalance.SortieFailGuardDeaths} guard deaths, +{GameBalance.SortieFailUnrest} Unrest ({failChance}%)";
    }

    public bool CanStart(GameState state)
    {
        if (!GameBalance.EnableSortieMission)
        {
            return false;
        }

        if (state.Flags.Fortification < 1)
        {
            return false;
        }

        return true;
    }

    public void ResolveOutcome(GameState state, ActiveMission mission, ResolutionEntry entry)
    {
        state.Flags.Tyranny.Add(1, lifetimeDays: 5);
        state.Flags.Fortification.Add(1, lifetimeDays: 5);
        var roll = state.RollPercent();

        if (roll <= GameBalance.SortieSuccessChance)
        {
            if (state.SiegeIntensity > 1)
            {
                state.SiegeIntensity -= GameBalance.SortieSuccessSiegeReduction;
            }

            state.SiegeEscalationDelayDays += GameBalance.SortieSuccessEscalationDelay;
            entry.Write(
                $"The sortie was a stunning success! Enemy siege engines destroyed. Siege intensity reduced. Escalation delayed by {GameBalance.SortieSuccessEscalationDelay} days.");
            return;
        }

        if (roll <= GameBalance.SortieSuccessChance + GameBalance.SortiePartialChance)
        {
            state.SiegeDamageMultiplier = GameBalance.SortiePartialDamageMultiplier;
            state.SiegeDamageReductionDaysRemaining = GameBalance.SortiePartialDurationDays;
            entry.Write(
                $"The sortie disrupted enemy positions. Siege damage reduced to {GameBalance.SortiePartialDamageMultiplier * 100:F0}% for {GameBalance.SortiePartialDurationDays} days.");
            return;
        }

        state.ApplyGuardDeath(GameBalance.SortieFailGuardDeaths, entry);
        state.AddUnrest(GameBalance.SortieFailUnrest, entry);
        entry.Write(
            $"The sortie was repulsed with heavy losses. {GameBalance.SortieFailGuardDeaths} guards fell in the failed assault.");
    }
}
