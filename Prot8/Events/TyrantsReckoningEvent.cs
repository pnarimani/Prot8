using Prot8.Simulation;

namespace Prot8.Events;

public sealed class TyrantsReckoningEvent : IRespondableEvent
{
    private const int MinDay = 20;

    public string Id => "tyrants_reckoning";
    public string Name => "Tyrant's Reckoning";

    public string Description =>
        "The people gather in the square, not in revolt — but in mourning. The weight of your rule has broken something in them. They look to you with hollow eyes. This is the moment of reckoning.";

    public bool ShouldTrigger(GameState state)
    {
        if (state.Flags.Tyranny < 8)
            return false;

        return state.Day >= MinDay;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        ResolveWithResponse("double_down", state, entry);
    }

    public IReadOnlyList<EventResponse> GetResponses(GameState state)
    {
        return
        [
            new EventResponse("double_down", "Double Down (auto-enact Martial Law if not active, -30 morale)"),
            new EventResponse("show_mercy", "Show Mercy (-20 unrest, +15 morale, Faith +2, Tyranny -3)"),
        ];
    }

    public void ResolveWithResponse(string responseId, GameState state, ResolutionEntry entry)
    {
        switch (responseId)
        {
            case "double_down":
                state.Flags.Tyranny.Add(1);
                state.AddMorale(-30, entry);
                if (!state.ActiveLawIds.Contains("martial_law"))
                {
                    state.ActiveLawIds.Add("martial_law");
                    state.Flags.MartialState.Set();
                    entry.Write("Martial Law is declared by force. Soldiers flood the streets. The people bow their heads. Hope dies quietly.");
                }
                else
                {
                    entry.Write("You double down on tyranny. The garrison tightens its grip. The people have nothing left to give but silence.");
                }
                break;

            case "show_mercy":
                state.AddUnrest(-20, entry);
                state.AddMorale(15, entry);
                state.Flags.Faith.Add(2);
                state.Flags.Tyranny.Add(-3);
                entry.Write("You kneel before the people. For the first time, you ask for forgiveness. Something shifts. The iron grip loosens. Perhaps there is still a path to redemption.");
                break;

            default:
                ResolveNow(state, entry);
                break;
        }
    }
}
