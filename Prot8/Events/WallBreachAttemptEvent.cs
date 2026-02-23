using Prot8.Simulation;

namespace Prot8.Events;

public sealed class WallBreachAttemptEvent : TriggeredEventBase
{
    private const int IntegrityThreshold = 30;
    private const int GuardsToDefend = 15;
    private const int IntegrityDamage = 15;

    public WallBreachAttemptEvent() : base("wall_breach_attempt", "Wall Breach Attempt",
        $"Triggers when perimeter integrity < {IntegrityThreshold}. -{IntegrityDamage} integrity (negated if guards >= {GuardsToDefend}).")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.ActivePerimeterZone.Integrity < IntegrityThreshold;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        if (state.Population.Guards >= GuardsToDefend)
        {
            report.Add(ReasonTags.Event, $"{Name}: guards held the breach and negated damage.");
            StartCooldown(state);
            return;
        }

        var perimeter = state.ActivePerimeterZone;
        perimeter.Integrity -= IntegrityDamage;
        report.Add(ReasonTags.Event, $"{Name}: {perimeter.Name} integrity -{IntegrityDamage}.");
        if (perimeter.Integrity <= 0)
        {
            StateChangeApplier.LoseZone(state, perimeter.Id, false, report);
        }

        StartCooldown(state);
    }
}