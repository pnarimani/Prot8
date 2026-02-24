using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class WellContaminationScareEvent : IRespondableEvent
{
    public string Id => "well_contamination_scare";
    public string Name => "Well Contamination Scare";

    public string Description =>
        "Word spreads through the city that the wells may have been poisoned by enemy agents. Whether true or not, the panic is real — and spreading fast.";

    const int TriggerDay = 5;

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
        var responses = new List<EventResponse>();

        if (state.Resources.Has(ResourceKind.Medicine, 5))
        {
            responses.Add(new EventResponse("medicine", "Treat with medicine"));
        }

        responses.Add(new EventResponse("boil", "Boil all water"));
        responses.Add(new EventResponse("ignore", "Ignore the warnings"));

        return responses;
    }

    public void ResolveWithResponse(string responseId, GameState state, ResolutionEntry entry)
    {
        switch (responseId)
        {
            case "medicine":
                entry.Write("Medicine purifies the wells. The worst is averted... for now.");
                state.AddResource(ResourceKind.Medicine, -5, entry);
                break;

            case "boil":
                entry.Write("You order all water boiled. It slows production, but limits the contamination.");
                state.AddSickness(1, entry);
                state.TaintedWellDaysRemaining = 1;
                break;

            default: // ignore
                entry.Write("Without action, sickness spreads through the water supply.");
                state.AddSickness(5, entry);
                break;
        }
    }
}