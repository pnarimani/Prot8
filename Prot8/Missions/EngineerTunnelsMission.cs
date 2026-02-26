using Prot8.Simulation;

namespace Prot8.Missions;

public sealed class EngineerTunnelsMission : IMissionDefinition
{
    private const int SuccessChance = 50;
    private const int PartialChance = 30;
    private const double SuccessDamageReduction = 0.6;
    private const int SuccessDuration = 5;
    private const double PartialDamageReduction = 0.8;
    private const int PartialDuration = 3;
    private const int FailDeaths = 4;
    private const int FailUnrest = 10;

    public string Id => "engineer_tunnels";
    public string Name => "Engineer Tunnels";
    public int DurationDays => 4;
    public int WorkerCost => 5;

    public string GetTooltip(GameState state) =>
        $"Siege damage x{SuccessDamageReduction} for {SuccessDuration}d ({SuccessChance}%) | x{PartialDamageReduction} for {PartialDuration}d ({PartialChance}%) | {FailDeaths} deaths, +{FailUnrest} unrest ({100 - SuccessChance - PartialChance}%). Requires Fortification >= 3.";

    public bool CanStart(GameState state)
    {
        if (state.Flags.Fortification < 3)
        {
            return false;
        }

        return true;
    }

    public void ResolveOutcome(GameState state, ActiveMission mission, ResolutionEntry entry)
    {
        state.Flags.Fortification.Add(1, lifetimeDays: 5);
        var roll = state.RollPercent();

        if (roll <= SuccessChance)
        {
            state.SiegeDamageMultiplier = SuccessDamageReduction;
            state.SiegeDamageReductionDaysRemaining = SuccessDuration;
            entry.Write($"The tunnels are complete! Engineers have undermined the enemy's siege works. Siege damage reduced to {SuccessDamageReduction * 100:F0}% for {SuccessDuration} days.");
            return;
        }

        if (roll <= SuccessChance + PartialChance)
        {
            state.SiegeDamageMultiplier = PartialDamageReduction;
            state.SiegeDamageReductionDaysRemaining = PartialDuration;
            entry.Write($"Partial success. The tunnels diverted some enemy efforts. Siege damage reduced to {PartialDamageReduction * 100:F0}% for {PartialDuration} days.");
            return;
        }

        state.ApplyDeath(FailDeaths, entry);
        state.AddUnrest(FailUnrest, entry);
        entry.Write($"Tunnel collapse! The earth swallowed {FailDeaths} engineers whole. The survivors crawl out trembling. The enemy laughs.");
    }
}
