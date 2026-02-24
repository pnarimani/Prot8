using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class WallBreachAttemptEvent : IRespondableEvent, ITriggeredEvent
{
    public string Id => "wall_breach";
    public string Name => "Wall Breach Attempt";
    public string Description => "The outer wall is crumbling. Enemy soldiers have gathered at the weakest section with battering rams and axes. The breach is imminent.";

    const int IntegrityThreshold = 30;

    public bool ShouldTrigger(GameState state)
    {
        return state.ActivePerimeterZone.Integrity < IntegrityThreshold;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        ResolveWithResponse("fall_back", state, entry);
    }

    public IReadOnlyList<EventResponse> GetResponses(GameState state)
    {
        var responses = new List<EventResponse>
        {
            new("reinforce", "Reinforce with guards"),
        };

        if (state.Resources.Has(ResourceKind.Materials, 10))
        {
            responses.Add(new EventResponse("barricade", "Barricade with materials"));
        }

        responses.Add(new EventResponse("fall_back", "Fall back"));

        return responses;
    }

    public void ResolveWithResponse(string responseId, GameState state, ResolutionEntry entry)
    {
        switch (responseId)
        {
            case "reinforce":
                if (state.Population.Guards >= 15)
                {
                    entry.Write(
                        "Guards form a wall of steel at the breach. The enemy crashes against them and falls back. The wall holds!");
                }
                else
                {
                    var perimeter = state.ActivePerimeterZone;
                    perimeter.Integrity -= 8;
                    entry.Write(
                        $"Too few guards to hold the line. The enemy carves a wound in {perimeter.Name}. The breach grows.");
                    if (perimeter.Integrity <= 0)
                    {
                        state.LoseZone(perimeter.Id, false, entry);
                    }
                }

                break;

            case "barricade":
                state.AddResource(ResourceKind.Materials, -10, entry);
                var barrPerimeter = state.ActivePerimeterZone;
                barrPerimeter.Integrity -= 5;
                entry.Write(
                    $"Barricades made from scavenged materials slow the enemy tide, but cannot stop them. {barrPerimeter.Name} sustains damage.");
                if (barrPerimeter.Integrity <= 0)
                {
                    state.LoseZone(barrPerimeter.Id, false, entry);
                }

                break;

            default: // fall_back
            {
                var perimeter = state.ActivePerimeterZone;
                perimeter.Integrity -= 15;
                entry.Write(
                    $"You order a full retreat. The enemy pours into the gap unchecked. {perimeter.Name} is torn apart as the garrison falls back.");
                if (perimeter.Integrity <= 0)
                {
                    state.LoseZone(perimeter.Id, false, entry);
                }

                break;
            }
        }

    }
}