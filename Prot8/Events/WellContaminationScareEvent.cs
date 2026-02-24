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

    public override void Apply(GameState state, ResolutionEntry entry)
    {
        ApplyResponse("ignore", state, entry);
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

    public void ApplyResponse(string responseId, GameState state, ResolutionEntry entry)
    {
        switch (responseId)
        {
            case "medicine":
                state.AddResource(ResourceKind.Medicine, -5, entry);
                state.AddSickness(2, entry);
                entry.Write($"{Name}: Medicine purifies the wells. The worst is averted — for now.");
                break;

            case "boil":
                state.AddSickness(3, entry);
                state.TaintedWellDaysRemaining = 1;
                entry.Write($"{Name}: You order all water boiled. It slows production, but limits the contamination.");
                break;

            default: // ignore
                state.AddSickness(5, entry);
                entry.Write($"{Name}: Without action, sickness spreads through the water supply.");
                break;
        }

        StartCooldown(state);
    }
}
