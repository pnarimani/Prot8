using Prot8.Constants;
using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class PlagueRatsEvent() : IRespondableEvent, ITriggeredEvent
{
    public string Id => "plague_rats";
    public string Name => "Plague Rats";
    public string Description => "Rats have infested the city in vast numbers, driven inward by the siege. They gnaw through food stores and carry disease wherever they nest.";

    const int TriggerDay = 23;

    public bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        ResolveWithResponse("nothing", state, entry);
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

    public void ResolveWithResponse(string responseId, GameState state, ResolutionEntry entry)
    {
        switch (responseId)
        {
            case "hunt":
                entry.Write(
                    "Organized hunts drive the rats from the quarter. But disease has already spread — some fall ill before the rodents are purged.");
                state.AddSickness(10, entry);
                state.ApplyDeath(2, entry);
                state.AddUnrest(5, entry);
                state.PlagueRatsActive = false;
                break;

            case "burn":
                entry.Write(
                    "You order the quarter burned. The flames purge the rats, but also consume precious materials. At least the plague is contained.");
                if (GameBalance.EnableHumanityScore) state.Flags.Humanity.Add(1);
                state.AddSickness(5, entry);
                state.AddResource(ResourceKind.Materials, -10, entry);
                state.PlagueRatsActive = false;
                break;

            default: // nothing
                entry.Write(
                    "You do nothing. The rats multiply and spread through the city. Disease follows in their wake, spreading faster each day.");
                if (GameBalance.EnableHumanityScore) state.Flags.Humanity.Add(-3);
                state.AddSickness(15, entry);
                state.ApplyDeath(3, entry);
                state.AddUnrest(10, entry);
                state.PlagueRatsActive = true;
                break;
        }

    }
}