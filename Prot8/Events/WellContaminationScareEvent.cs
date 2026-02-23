using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class WellContaminationScareEvent : TriggeredEventBase, IRespondableEvent
{
    private const int TriggerDay = 5;

    public WellContaminationScareEvent() : base("well_contamination", "Well Contamination Scare",
        "Reports of fouled water spread through the city. The wells may be contaminated.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        ApplyResponse("ignore", state, report);
    }

    public IReadOnlyList<EventResponse> GetResponses(GameState state)
    {
        var responses = new List<EventResponse>();

        if (state.Resources.Has(ResourceKind.Medicine, 5))
        {
            responses.Add(new EventResponse("medicine", "Treat with medicine"));
        }

        responses.Add(new EventResponse("boil", "Boil all water"));
        responses.Add(new EventResponse("ignore", "Ignore the warnings"));

        return responses;
    }

    public void ApplyResponse(string responseId, GameState state, DayResolutionReport report)
    {
        switch (responseId)
        {
            case "medicine":
                StateChangeApplier.AddResource(state, ResourceKind.Medicine, -5, report, ReasonTags.Event, Name);
                StateChangeApplier.AddSickness(state, 2, report, ReasonTags.Event, Name);
                report.Add(ReasonTags.Event, $"{Name}: Medicine purifies the wells. The worst is averted — for now.");
                break;

            case "boil":
                StateChangeApplier.AddSickness(state, 3, report, ReasonTags.Event, Name);
                state.TaintedWellDaysRemaining = 1;
                report.Add(ReasonTags.Event, $"{Name}: You order all water boiled. It slows production, but limits the contamination.");
                break;

            default: // ignore
                StateChangeApplier.AddSickness(state, 5, report, ReasonTags.Event, Name);
                report.Add(ReasonTags.Event, $"{Name}: Without action, sickness spreads through the water supply.");
                break;
        }

        StartCooldown(state);
    }
}
