using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class RefugeesAtTheGatesEvent : TriggeredEventBase
{
    private const int TriggerDay = 12;
    private const int HealthyWorkers = 5;
    private const int SickWorkers = 3;
    private const int UnrestGain = 5;
    private const int MoraleGain = 3;

    public RefugeesAtTheGatesEvent() : base("refugees_at_gates", "Refugees at the Gates",
        "Day 12: 8 refugees arrive (5 healthy, 3 sick). +5 unrest, +3 morale.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        state.Population.HealthyWorkers += HealthyWorkers;
        var recoveryDays = GameBalance.ComputeRecoveryDays(state.Sickness);
        state.Population.AddSickWorkers(SickWorkers, recoveryDays);

        StateChangeApplier.AddUnrest(state, UnrestGain, report, ReasonTags.Event, $"{Name} overcrowding");
        StateChangeApplier.AddMorale(state, MoraleGain, report, ReasonTags.Event, $"{Name} solidarity");

        report.Add(ReasonTags.Event, $"{Name}: {HealthyWorkers + SickWorkers} refugees admitted. +{HealthyWorkers} healthy workers, +{SickWorkers} sick.");
        StartCooldown(state);
    }
}
