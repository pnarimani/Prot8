using Prot8.Simulation;

namespace Prot8.Events;

public sealed class MilitiaVolunteersEvent : IRespondableEvent
{
    public string Id => "militia_volunteers";
    public string Name => "Militia Volunteers";

    public string Description =>
        "A group of workers arrives at the garrison gate armed with farming tools and grim determination. They are not soldiers. But they want to fight.";

    const int TriggerDay = 6;

    public bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay && state.Population.HealthyWorkers >= 3;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        ResolveWithResponse("accept", state, entry);
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

    public void ResolveWithResponse(string responseId, GameState state, ResolutionEntry entry)
    {
        switch (responseId)
        {
            case "accept":
            {
                var converted = state.Population.ConvertHealthyToGuards(3);
                state.Allocation.RemoveWorkersProportionally(converted);
                entry.Write(
                    $"You accept their offer. {converted} workers take up arms. 'We'd rather fight than starve behind walls,' they say.");
                break;
            }

            case "decline":
                entry.Write(
                    "You turn them down gently, citing the need for workers. The volunteers appreciate being valued, and return to their posts.");
                state.AddMorale(3, entry);
                break;

            default: // conscript
            {
                var converted = state.Population.ConvertHealthyToGuards(5);
                state.Allocation.RemoveWorkersProportionally(converted);
                entry.Write(
                    "You conscript even more than they offered. Workers are dragged from their tasks to fill the garrison. Unrest spreads among those left behind.");
                state.AddUnrest(5, entry);
                break;
            }
        }
    }
}