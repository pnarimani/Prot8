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

    public override void Apply(GameState state, ResolutionEntry entry)
    {
        ApplyResponse("accept", state, entry);
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

    public void ApplyResponse(string responseId, GameState state, ResolutionEntry entry)
    {
        switch (responseId)
        {
            case "accept":
            {
                var converted = state.Population.ConvertHealthyToGuards(3);
                state.Allocation.RemoveWorkersProportionally(converted);
                entry.Write($"{Name}: {converted} workers take up arms. \"We'd rather fight than starve behind walls.\"");
                break;
            }

            case "decline":
                state.AddMorale(3, entry);
                entry.Write($"{Name}: You turn them down gently. The workers appreciate being valued.");
                break;

            default: // conscript
            {
                var converted = state.Population.ConvertHealthyToGuards(5);
                state.Allocation.RemoveWorkersProportionally(converted);
                state.AddUnrest(5, entry);
                break;
            }
        }

        StartCooldown(state);
    }
}
