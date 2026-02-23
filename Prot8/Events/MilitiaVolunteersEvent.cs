using Prot8.Simulation;

namespace Prot8.Events;

public sealed class MilitiaVolunteersEvent : TriggeredEventBase, IRespondableEvent
{
    private const int TriggerDay = 6;

    public MilitiaVolunteersEvent() : base("militia_volunteers", "Militia Volunteers",
        "A group of workers approaches, asking to join the guard. They say they'd rather fight than starve behind walls.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay && state.Population.HealthyWorkers >= 3;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        ApplyResponse("accept", state, report);
    }

    public IReadOnlyList<EventResponse> GetResponses(GameState state)
    {
        return
        [
            new EventResponse("accept", "Accept the volunteers"),
            new EventResponse("decline", "Decline, workers are needed"),
            new EventResponse("conscript", "Conscript even more"),
        ];
    }

    public void ApplyResponse(string responseId, GameState state, DayResolutionReport report)
    {
        switch (responseId)
        {
            case "accept":
            {
                var converted = state.Population.ConvertHealthyToGuards(3);
                state.Allocation.RemoveWorkersProportionally(converted);
                report.Add(ReasonTags.Event, $"{Name}: {converted} workers take up arms. \"We'd rather fight than starve behind walls.\"");
                break;
            }

            case "decline":
                StateChangeApplier.AddMorale(state, 3, report, ReasonTags.Event, Name);
                report.Add(ReasonTags.Event, $"{Name}: You turn them down gently. The workers appreciate being valued.");
                break;

            default: // conscript
            {
                var converted = state.Population.ConvertHealthyToGuards(5);
                state.Allocation.RemoveWorkersProportionally(converted);
                StateChangeApplier.AddUnrest(state, 5, report, ReasonTags.Event, Name);
                report.Add(ReasonTags.Event, $"{Name}: You conscript {converted} workers by force. The people murmur their displeasure.");
                break;
            }
        }

        StartCooldown(state);
    }
}
