using Prot8.Simulation;

namespace Prot8.Events;

public sealed class WallBreachAttemptEvent : TriggeredEventBase
{
    public WallBreachAttemptEvent() : base("wall_breach_attempt", "Wall Breach Attempt", "Trigger when active perimeter integrity is below 30.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.ActivePerimeterZone.Integrity < 30;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        if (state.Population.Guards >= 15)
        {
            report.Add(ReasonTags.Event, $"{Name}: guards held the breach and negated damage.");
            StartCooldown(state);
            return;
        }

        var perimeter = state.ActivePerimeterZone;
        perimeter.Integrity -= 15;
        report.Add(ReasonTags.Event, $"{Name}: {perimeter.Name} integrity -15.");
        if (perimeter.Integrity <= 0)
        {
            StateChangeApplier.LoseZone(state, perimeter.Id, false, report);
        }

        StartCooldown(state);
    }
}