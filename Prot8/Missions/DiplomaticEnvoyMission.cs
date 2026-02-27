using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Missions;

public sealed class DiplomaticEnvoyMission : IMissionDefinition
{
    private const int SuccessChance = 40;
    private const int PartialChance = 30;
    private const int SuccessDelay = 5;
    private const int PartialDelay = 2;
    private const int FailDeaths = 3;
    private const int FailUnrest = 10;

    public string Id => "diplomatic_envoy";
    public string Name => "Diplomatic Envoy";
    public int DurationDays => 3;
    public int WorkerCost => 3;

    public string GetTooltip(GameState state) =>
        $"Siege delay +{SuccessDelay}d ({SuccessChance}%) | +{PartialDelay}d ({PartialChance}%) | {FailDeaths} deaths, +{FailUnrest} unrest ({100 - SuccessChance - PartialChance}%). Requires Faith >= 3.";

    public bool CanStart(GameState state)
    {
        if (state.Flags.Faith < 3)
        {
            return false;
        }

        return true;
    }

    public void ResolveOutcome(GameState state, ActiveMission mission, ResolutionEntry entry)
    {
        state.Flags.Faith.Add(1, lifetimeDays: 5);
        var roll = state.RollPercent();

        if (roll <= SuccessChance)
        {
            state.SiegeEscalationDelayDays += SuccessDelay;
            if (GameBalance.EnableReliefArmy && state.ReliefAcceleration < GameBalance.MaxReliefAcceleration)
            {
                state.ReliefAcceleration++;
                entry.Write("The envoy's negotiations buy time — and word reaches allies. Relief accelerated by 1 day.");
            }
            entry.Write($"The envoy spoke with eloquence and conviction. The enemy commander hesitates. Siege escalation delayed by {SuccessDelay} days.");
            return;
        }

        if (roll <= SuccessChance + PartialChance)
        {
            state.SiegeEscalationDelayDays += PartialDelay;
            entry.Write($"Negotiations bore some fruit. A temporary truce is agreed. Siege escalation delayed by {PartialDelay} days.");
            return;
        }

        state.ApplyDeath(FailDeaths, entry);
        state.AddUnrest(FailUnrest, entry);
        entry.Write($"The envoy was seized and executed. Their heads were catapulted back over the walls. {FailDeaths} dead. The people despair.");
    }
}
