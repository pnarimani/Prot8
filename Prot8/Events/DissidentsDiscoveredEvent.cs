using Prot8.Simulation;

namespace Prot8.Events;

public sealed class DissidentsDiscoveredEvent : IRespondableEvent
{
    private const int TriggerChance = 20;
    private const int MinDay = 10;

    public string Id => "dissidents_discovered";
    public string Name => "Dissidents Discovered";

    public string Description =>
        "A cell of dissidents has been uncovered hiding in the lower district. They have been spreading seditious pamphlets and organizing secret meetings.";

    public bool ShouldTrigger(GameState state)
    {
        if (state.Flags.Tyranny < 4)
            return false;

        if (state.Day < MinDay)
            return false;

        return state.RollPercent() <= TriggerChance;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        ResolveWithResponse("imprison", state, entry);
    }

    public IReadOnlyList<EventResponse> GetResponses(GameState state)
    {
        return
        [
            new EventResponse("execute", "Execute them (-15 unrest, 3 deaths, -5 morale)"),
            new EventResponse("imprison", "Imprison them (-10 unrest, +5 morale)"),
            new EventResponse("release", "Release them (+5 morale, +8 unrest)"),
        ];
    }

    public void ResolveWithResponse(string responseId, GameState state, ResolutionEntry entry)
    {
        switch (responseId)
        {
            case "execute":
                state.AddUnrest(-15, entry);
                state.ApplyDeath(3, entry);
                state.AddMorale(-5, entry);
                state.Flags.Tyranny.Add(1);
                state.Flags.FearLevel.Add(1);
                entry.Write("The dissidents are dragged to the square and executed publicly. The message is clear: dissent means death.");
                break;

            case "imprison":
                state.AddUnrest(-10, entry);
                state.AddMorale(5, entry);
                entry.Write("The dissidents are imprisoned. The people feel safer, and some appreciate the restraint.");
                break;

            case "release":
                state.AddMorale(5, entry);
                state.AddUnrest(8, entry);
                state.Flags.Faith.Add(1);
                entry.Write("You release the dissidents with a warning. Some call it mercy, others call it weakness. The faithful see it as grace.");
                break;

            default:
                ResolveNow(state, entry);
                break;
        }
    }
}
