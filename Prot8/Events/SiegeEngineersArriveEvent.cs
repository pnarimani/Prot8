using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class SiegeEngineersArriveEvent : IRespondableEvent
{
    private const int TriggerChance = 25;
    private const int MinDay = 15;

    public string Id => "siege_engineers_arrive";
    public string Name => "Siege Engineers Arrive";

    public string Description =>
        "A band of wandering engineers appears at the gate, fleeing the enemy's advance. They offer their expertise in exchange for refuge and food.";

    public bool ShouldTrigger(GameState state)
    {
        if (state.Flags.Fortification < 5)
            return false;

        if (state.Day < MinDay)
            return false;

        return state.RollPercent() <= TriggerChance;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        ResolveWithResponse("decline", state, entry);
    }

    public IReadOnlyList<EventResponse> GetResponses(GameState state)
    {
        return
        [
            new EventResponse("accept", "Accept (+3 workers, +20 materials, -10 food)"),
            new EventResponse("decline", "Decline (+5 morale)"),
        ];
    }

    public void ResolveWithResponse(string responseId, GameState state, ResolutionEntry entry)
    {
        switch (responseId)
        {
            case "accept":
                state.Population.HealthyWorkers += 3;
                state.AddResource(ResourceKind.Materials, 20, entry);
                state.AddResource(ResourceKind.Food, -10, entry);
                state.Flags.Fortification.Add(1);
                entry.Write("The engineers are welcomed. They bring tools, knowledge, and hands hardened by siege work. The walls grow stronger. The food stores grow thinner.");
                break;

            case "decline":
                state.AddMorale(5, entry);
                entry.Write("You turn the engineers away. The people appreciate the caution — strangers in times of siege are rarely what they seem.");
                break;

            default:
                ResolveNow(state, entry);
                break;
        }
    }
}
