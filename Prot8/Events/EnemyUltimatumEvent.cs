using Prot8.Simulation;

namespace Prot8.Events;

public sealed class EnemyUltimatumEvent : TriggeredEventBase, IRespondableEvent
{
    private const int TriggerDay = 30;

    public EnemyUltimatumEvent() : base("enemy_ultimatum", "Enemy Ultimatum",
        "The enemy commander demands your surrender. Civilians question whether resistance is worth the cost.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay;
    }

    public override void Apply(GameState state, ResolutionEntry entry)
    {
        ApplyResponse("ignore", state, entry);
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

    public void ApplyResponse(string responseId, GameState state, ResolutionEntry entry)
    {
        switch (responseId)
        {
            case "defy":
                state.AddMorale(10, entry);
                state.AddUnrest(15, entry);
                entry.Write($"{Name}: You rally the people with fiery words. Spirits lift, but the hotheads grow bolder.");
                break;

            case "negotiate":
                state.AddMorale(-5, entry);
                state.AddUnrest(5, entry);
                state.ApplyWorkerDesertion(2);
                entry.Write($"{Name}: You buy time, but the appearance of weakness emboldens deserters.");
                break;

            default: // ignore
                state.AddMorale(-15, entry);
                state.AddUnrest(20, entry);
                state.ApplyWorkerDesertion(5);
                entry.Write($"{Name}: Silence is taken as weakness. Panic spreads through the ranks.");
                break;
        }

        StartCooldown(state);
    }
}
