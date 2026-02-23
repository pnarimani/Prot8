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

    public override void Apply(GameState state, DayResolutionReport report)
    {
        // Default: accept the trade (backward compat for non-interactive callers)
        ApplyResponse("accept", state, report);
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

    public void ApplyResponse(string responseId, GameState state, DayResolutionReport report)
    {
        switch (responseId)
        {
            case "accept":
                StateChangeApplier.AddResource(state, ResourceKind.Food, 20, report, ReasonTags.Event, Name);
                StateChangeApplier.AddResource(state, ResourceKind.Materials, -MaterialsCost, report, ReasonTags.Event, Name);
                report.Add(ReasonTags.Event, $"{Name}: You accept the smuggler's offer. Food for materials — a fair trade in desperate times.");
                break;

            case "demand":
                StateChangeApplier.AddResource(state, ResourceKind.Food, 25, report, ReasonTags.Event, Name);
                StateChangeApplier.AddResource(state, ResourceKind.Materials, -MaterialsCost, report, ReasonTags.Event, Name);
                StateChangeApplier.AddUnrest(state, 5, report, ReasonTags.Event, Name);
                report.Add(ReasonTags.Event, $"{Name}: You press the smuggler for more. He complies, but word of your heavy-handedness spreads.");
                break;

            default: // refuse
                report.Add(ReasonTags.Event, $"{Name}: You turn the smuggler away. He vanishes back into the night.");
                break;
        }

        StartCooldown(state);
    }
}
