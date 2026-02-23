using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class WallBreachAttemptEvent : TriggeredEventBase, IRespondableEvent
{
    private const int IntegrityThreshold = 30;

    public WallBreachAttemptEvent() : base("wall_breach_attempt", "Wall Breach Attempt",
        "The enemy has found a weak point in the wall and is attempting to break through.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.ActivePerimeterZone.Integrity < IntegrityThreshold;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        ApplyResponse("fall_back", state, report);
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

    public void ApplyResponse(string responseId, GameState state, DayResolutionReport report)
    {
        switch (responseId)
        {
            case "reinforce":
                if (state.Population.Guards >= 15)
                {
                    report.Add(ReasonTags.Event, $"{Name}: Guards held the breach and negated the damage.");
                }
                else
                {
                    var perimeter = state.ActivePerimeterZone;
                    perimeter.Integrity -= 8;
                    report.Add(ReasonTags.Event, $"{Name}: Not enough guards to hold the line. {perimeter.Name} integrity -8.");
                    if (perimeter.Integrity <= 0)
                    {
                        StateChangeApplier.LoseZone(state, perimeter.Id, false, report);
                    }
                }
                break;

            case "barricade":
                StateChangeApplier.AddResource(state, ResourceKind.Materials, -10, report, ReasonTags.Event, Name);
                var barrPerimeter = state.ActivePerimeterZone;
                barrPerimeter.Integrity -= 5;
                report.Add(ReasonTags.Event, $"{Name}: Barricades slow the breach. {barrPerimeter.Name} integrity -5.");
                if (barrPerimeter.Integrity <= 0)
                {
                    StateChangeApplier.LoseZone(state, barrPerimeter.Id, false, report);
                }
                break;

            default: // fall_back
            {
                var perimeter = state.ActivePerimeterZone;
                perimeter.Integrity -= 15;
                report.Add(ReasonTags.Event, $"{Name}: Full retreat. {perimeter.Name} integrity -15.");
                if (perimeter.Integrity <= 0)
                {
                    StateChangeApplier.LoseZone(state, perimeter.Id, false, report);
                }
                break;
            }
        }

        StartCooldown(state);
    }
}
