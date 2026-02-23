using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class RefugeesAtTheGatesEvent : TriggeredEventBase, IRespondableEvent
{
    private const int TriggerDay = 12;

    public RefugeesAtTheGatesEvent() : base("refugees_at_gates", "Refugees at the Gates",
        "A desperate group of refugees huddles at the gate, some clearly sick. They beg to be let in.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        ApplyResponse("turn_away", state, report);
    }

    public IReadOnlyList<EventResponse> GetResponses(GameState state)
    {
        return
        [
            new EventResponse("open", "Open the gates"),
            new EventResponse("healthy_only", "Admit only the healthy"),
            new EventResponse("turn_away", "Turn them away"),
        ];
    }

    public void ApplyResponse(string responseId, GameState state, DayResolutionReport report)
    {
        switch (responseId)
        {
            case "open":
                state.Population.HealthyWorkers += 5;
                var recoveryDays = GameBalance.ComputeRecoveryDays(state.Sickness);
                state.Population.AddSickWorkers(3, recoveryDays);
                StateChangeApplier.AddUnrest(state, 5, report, ReasonTags.Event, $"{Name} overcrowding");
                StateChangeApplier.AddMorale(state, 3, report, ReasonTags.Event, $"{Name} solidarity");
                report.Add(ReasonTags.Event, $"{Name}: 8 refugees admitted. +5 healthy workers, +3 sick. The city opens its arms.");
                break;

            case "healthy_only":
                state.Population.HealthyWorkers += 5;
                StateChangeApplier.AddUnrest(state, 3, report, ReasonTags.Event, $"{Name} selection");
                report.Add(ReasonTags.Event, $"{Name}: Only the healthy are admitted. The sick are turned away, their cries echoing beyond the walls.");
                break;

            default: // turn_away
                StateChangeApplier.AddMorale(state, -10, report, ReasonTags.Event, Name);
                StateChangeApplier.AddUnrest(state, 5, report, ReasonTags.Event, Name);
                report.Add(ReasonTags.Event, $"{Name}: The gates remain closed. The refugees scatter into the wasteland. Morale plummets.");
                break;
        }

        StartCooldown(state);
    }
}
