using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class PlagueRatsEvent : TriggeredEventBase, IRespondableEvent
{
    private const int TriggerDay = 18;

    public PlagueRatsEvent() : base("plague_rats", "Plague Rats",
        "Rats swarm through the lower quarters, spreading disease. The people demand action.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay;
    }

    public override void Apply(GameState state, ResolutionEntry entry)
    {
        ApplyResponse("nothing", state, entry);
    }

    public IReadOnlyList<EventResponse> GetResponses(GameState state)
    {
        return
        [
            new EventResponse("hunt", "Organize rat hunts"),
            new EventResponse("burn", "Burn the infested quarter"),
            new EventResponse("nothing", "Do nothing"),
        ];
    }

    public void ApplyResponse(string responseId, GameState state, ResolutionEntry entry)
    {
        switch (responseId)
        {
            case "hunt":
                state.AddSickness(10, entry);
                state.ApplyDeath(2, entry);
                state.AddUnrest(5, entry);
                state.PlagueRatsActive = false;
                entry.Write($"{Name}: Organized hunts contain the rats, but not before disease claims lives.");
                break;

            case "burn":
                state.AddSickness(5, entry);
                state.AddResource(ResourceKind.Materials, -10, entry);
                state.PlagueRatsActive = false;
                entry.Write($"{Name}: Fire purges the infested quarter. The rats are gone, but so are precious supplies.");
                break;

            default: // nothing
                state.AddSickness(15, entry);
                state.ApplyDeath(3, entry);
                state.AddUnrest(10, entry);
                state.PlagueRatsActive = true;
                entry.Write($"{Name}: Rats carry disease into the inner city. Sickness will spread faster from now on.");
                break;
        }

        StartCooldown(state);
    }
}
