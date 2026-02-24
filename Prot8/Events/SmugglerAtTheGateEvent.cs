using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class SmugglerAtTheGateEvent : TriggeredEventBase, IRespondableEvent
{
    private const int TriggerDay = 3;
    private const int MaterialsCost = 15;

    public SmugglerAtTheGateEvent() : base("smuggler_gate", "Smuggler at the Gate",
        "A smuggler slips through the siege lines, offering food in exchange for materials.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay;
    }

    public override void Apply(GameState state, ResolutionEntry entry)
    {
        // Default: accept the trade (backward compat for non-interactive callers)
        ApplyResponse("accept", state, entry);
    }

    public IReadOnlyList<EventResponse> GetResponses(GameState state)
    {
        return
        [
            new EventResponse("accept", "Accept the trade"),
            new EventResponse("demand", "Demand a better deal"),
            new EventResponse("refuse", "Turn him away"),
        ];
    }

    public void ApplyResponse(string responseId, GameState state, ResolutionEntry entry)
    {
        switch (responseId)
        {
            case "accept":
                state.AddResource(ResourceKind.Food, 20, entry);
                state.AddResource(ResourceKind.Materials, -MaterialsCost, entry);
                entry.Write($"{Name}: You accept the smuggler's offer. Food for materials — a fair trade in desperate times.");
                break;

            case "demand":
                state.AddResource(ResourceKind.Food, 25, entry);
                state.AddResource(ResourceKind.Materials, -MaterialsCost, entry);
                state.AddUnrest(5, entry);
                entry.Write($"{Name}: You press the smuggler for more. He complies, but word of your heavy-handedness spreads.");
                break;

            default: // refuse
                entry.Write($"{Name}: You turn the smuggler away. He vanishes back into the night.");
                break;
        }

        StartCooldown(state);
    }
}
