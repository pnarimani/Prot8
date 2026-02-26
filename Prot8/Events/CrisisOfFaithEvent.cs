using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class CrisisOfFaithEvent : IRespondableEvent
{
    private const int MinDay = 15;

    public string Id => "crisis_of_faith";
    public string Name => "Crisis of Faith";

    public string Description =>
        "The faithful gather at the temple, but the prayers ring hollow. Morale is low, and doubt spreads like plague. The priests look to you for guidance — but their eyes are uncertain.";

    public bool ShouldTrigger(GameState state)
    {
        if (state.Flags.Faith < 6)
            return false;

        if (state.Morale >= 30)
            return false;

        return state.Day >= MinDay;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        ResolveWithResponse("abandon", state, entry);
    }

    public IReadOnlyList<EventResponse> GetResponses(GameState state)
    {
        return
        [
            new EventResponse("vigil", "Hold Vigil (+20 morale, -10 food, +5 sickness)"),
            new EventResponse("abandon", "Abandon Faith (-5 morale, +10 unrest, Faith -3)"),
        ];
    }

    public void ResolveWithResponse(string responseId, GameState state, ResolutionEntry entry)
    {
        switch (responseId)
        {
            case "vigil":
                state.AddMorale(20, entry);
                state.AddResource(ResourceKind.Food, -10, entry);
                state.AddSickness(5, entry);
                state.Flags.Faith.Add(1);
                entry.Write("An all-night vigil is held. Candles burn until dawn. The faithful emerge weary but renewed. Sickness spreads in the crowded temple, but spirits are lifted.");
                break;

            case "abandon":
                state.AddMorale(-5, entry);
                state.AddUnrest(10, entry);
                state.Flags.Faith.Add(-3);
                entry.Write("You declare the temple closed. The faithful scatter. Some weep, others rage. The foundations of belief crumble. What replaces them may be worse.");
                break;

            default:
                ResolveNow(state, entry);
                break;
        }
    }
}
