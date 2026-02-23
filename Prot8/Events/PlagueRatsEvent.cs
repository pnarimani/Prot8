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

    public override void Apply(GameState state, DayResolutionReport report)
    {
        ApplyResponse("nothing", state, report);
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

    public void ApplyResponse(string responseId, GameState state, DayResolutionReport report)
    {
        switch (responseId)
        {
            case "hunt":
                StateChangeApplier.AddSickness(state, 10, report, ReasonTags.Event, Name);
                StateChangeApplier.ApplyDeaths(state, 2, report, ReasonTags.Event, Name);
                StateChangeApplier.AddUnrest(state, 5, report, ReasonTags.Event, $"{Name} panic");
                state.PlagueRatsActive = false;
                report.Add(ReasonTags.Event, $"{Name}: Organized hunts contain the rats, but not before disease claims lives.");
                break;

            case "burn":
                StateChangeApplier.AddSickness(state, 5, report, ReasonTags.Event, Name);
                StateChangeApplier.AddResource(state, ResourceKind.Materials, -10, report, ReasonTags.Event, Name);
                state.PlagueRatsActive = false;
                report.Add(ReasonTags.Event, $"{Name}: Fire purges the infested quarter. The rats are gone, but so are precious supplies.");
                break;

            default: // nothing
                StateChangeApplier.AddSickness(state, 15, report, ReasonTags.Event, Name);
                StateChangeApplier.ApplyDeaths(state, 3, report, ReasonTags.Event, Name);
                StateChangeApplier.AddUnrest(state, 10, report, ReasonTags.Event, $"{Name} panic");
                state.PlagueRatsActive = true;
                report.Add(ReasonTags.Event, $"{Name}: Rats carry disease into the inner city. Sickness will spread faster from now on.");
                break;
        }

        StartCooldown(state);
    }
}
