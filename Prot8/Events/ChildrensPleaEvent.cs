using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class ChildrensPleaEvent : IRespondableEvent
{
    private const int TriggerChance = 15;
    private const int MinDay = 12;

    public string Id => "childrens_plea";
    public string Name => "Children's Plea";

    public string Description =>
        "A group of orphaned children approaches the keep, begging for shelter. They are gaunt, wide-eyed, and shivering. The crowd watches to see what you will do.";

    public bool ShouldTrigger(GameState state)
    {
        if (state.Flags.Faith < 3)
            return false;

        if (state.Day < MinDay)
            return false;

        return state.RollPercent() <= TriggerChance;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        ResolveWithResponse("refuse", state, entry);
    }

    public IReadOnlyList<EventResponse> GetResponses(GameState state)
    {
        return
        [
            new EventResponse("grant", "Grant Shelter (-10 materials, +10 morale, +3 sickness)"),
            new EventResponse("refuse", "Refuse (-5 morale, +5 unrest)"),
        ];
    }

    public void ResolveWithResponse(string responseId, GameState state, ResolutionEntry entry)
    {
        switch (responseId)
        {
            case "grant":
                state.AddResource(ResourceKind.Materials, -10, entry);
                state.AddMorale(10, entry);
                state.AddSickness(3, entry);
                state.Flags.Faith.Add(1);
                entry.Write("You take the children in. Shelters are hastily built. The people smile through their tears. Sickness spreads in the crowded quarters, but hearts are warmer.");
                break;

            case "refuse":
                state.AddMorale(-5, entry);
                state.AddUnrest(5, entry);
                state.Flags.Tyranny.Add(1);
                entry.Write("You turn the children away. They wander back into the ruins. The crowd disperses in silence. Something has broken in their eyes.");
                break;

            default:
                ResolveNow(state, entry);
                break;
        }
    }
}
