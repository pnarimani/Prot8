using Prot8.Simulation;

namespace Prot8.Events;

public sealed class EnemyUltimatumEvent : IRespondableEvent
{
    public string Id => "enemy_ultimatum";
    public string Name => "Enemy Ultimatum";

    public string Description =>
        "A messenger arrives under flag of truce with a message from the enemy commander: surrender the city or face total annihilation. He gives you until dawn to decide.";

    const int TriggerDay = 30;

    public bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        ResolveWithResponse("ignore", state, entry);
    }

    public IReadOnlyList<EventResponse> GetResponses(GameState state)
    {
        return
        [
            new EventResponse("defy", "Defy them publicly"),
            new EventResponse("negotiate", "Negotiate for time"),
            new EventResponse("ignore", "Ignore the ultimatum"),
        ];
    }

    public void ResolveWithResponse(string responseId, GameState state, ResolutionEntry entry)
    {
        switch (responseId)
        {
            case "defy":
                entry.Write(
                    """
                    You stand on the walls and shout defiance.
                    The people rally to your voice, but some hear only war, and grow restless for bloodshed.
                    """
                );
                state.AddMorale(10, entry);
                state.AddUnrest(15, entry);
                break;

            case "negotiate":
                entry.Write(
                    """
                    You send envoys to treat with the enemy. 
                    The people see weakness in negotiation. 
                    Some desert rather than face a doomed stand.
                    """
                );
                state.AddMorale(-5, entry);
                state.AddUnrest(5, entry);
                state.ApplyWorkerDesertion(2);
                break;

            default: // ignore
                entry.Write(
                    """
                    You refuse to answer.
                    Silence is interpreted as cowardice. 
                    Hope drains from the city like blood from a wound.
                    """
                );
                state.AddMorale(-15, entry);
                state.AddUnrest(20, entry);
                state.ApplyWorkerDesertion(5);
                break;
        }
    }
}