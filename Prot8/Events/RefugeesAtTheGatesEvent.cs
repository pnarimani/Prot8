using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class RefugeesAtTheGatesEvent() : IRespondableEvent
{
    public string Id => "refugees_at_gates";
    public string Name => "Refugees at the Gates";
    public string Description => "A ragged column of survivors has arrived at your gates — families, the elderly, the wounded. They fled villages the enemy burned and now beg for sanctuary.";

    const int TriggerDay = 12;

    public bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        ResolveWithResponse("turn_away", state, entry);
    }

    public IReadOnlyList<EventResponse> GetResponses(GameState state)
    {
        return
        [
            new EventResponse("open", "Open the gates"),
            new EventResponse("healthy_only", "Admit only the healthy"),
            new EventResponse("turn_away", "Turn them away"),
        ];
    }

    public void ResolveWithResponse(string responseId, GameState state, ResolutionEntry entry)
    {
        switch (responseId)
        {
            case "open":
                entry.Write(
                    "You open the gates. Eight refugees stream in." +
                    "Five healthy, three already coughing. " +
                    "Humanity triumphs over caution, for better or worse.");
                if (GameBalance.EnableHumanityScore) state.Flags.Humanity.Add(3);
                state.Population.HealthyWorkers += 5;
                var recoveryDays = GameBalance.ComputeRecoveryDays(state.Sickness);
                state.Population.AddSickWorkers(3, recoveryDays);
                state.AddUnrest(5, entry);
                state.AddMorale(3, entry);
                break;

            case "healthy_only":
                entry.Write(
                    "You admit only those who appear healthy. " +
                    "The sick are turned away. " +
                    "Their pleas fade into the distance as the gates close.");
                state.Population.HealthyWorkers += 5;
                state.AddUnrest(3, entry);
                break;

            default: // turn_away
                entry.Write(
                    "You turn them all away. The gates remain sealed. The refugees vanish into the night, and the city earns a cruel reputation.");
                state.AddMorale(-10, entry);
                state.AddUnrest(5, entry);
                if (GameBalance.EnableHumanityScore) state.Flags.Humanity.Add(-5);
                break;
        }

    }
}