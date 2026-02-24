using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class SmugglerAtTheGateEvent : IRespondableEvent
{
    public string Id => "smuggler_at_gate";
    public string Name => "Smuggler at the Gate";

    public string Description =>
        "A hooded figure taps at the postern door with unusual confidence. A smuggler — offering food for materials. Someone is already making a living from your suffering.";

    const int TriggerDay = 3;
    const int MaterialsCost = 15;
    const int FairFoodAmount = 20;
    const int ForceFoodAmount = 30;

    public bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        ResolveWithResponse("refuse", state, entry);
    }

    public IReadOnlyList<EventResponse> GetResponses(GameState state)
    {
        return
        [
            new EventResponse("accept", "Accept the trade", $"{FairFoodAmount} food for {MaterialsCost} materials"),
            new EventResponse("demand", "Demand a better deal",
                $"Ask for {ForceFoodAmount} food for {MaterialsCost} materials instead"),
            new EventResponse("refuse", "Turn him away"),
        ];
    }

    public void ResolveWithResponse(string responseId, GameState state, ResolutionEntry entry)
    {
        switch (responseId)
        {
            case "accept":
                entry.Write(
                    "You accept the smuggler's offer. Food for materials — a fair trade in desperate times. He slips away before the guards notice.");
                state.AddResource(ResourceKind.Food, FairFoodAmount, entry);
                state.AddResource(ResourceKind.Materials, -MaterialsCost, entry);
                break;

            case "demand":
                entry.Write(
                    "You press the smuggler for more. He complies, but the transaction is uglier now. Word of your heavy-handedness spreads through the market alleys.");
                state.AddResource(ResourceKind.Food, ForceFoodAmount, entry);
                state.AddResource(ResourceKind.Materials, -MaterialsCost, entry);
                state.AddUnrest(5, entry);
                break;

            default: // refuse
                entry.Write("You turn the smuggler away. He vanishes back into the night.");
                break;
        }
    }
}