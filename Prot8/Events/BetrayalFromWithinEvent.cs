using Prot8.Simulation;

namespace Prot8.Events;

public sealed class BetrayalFromWithinEvent : TriggeredEventBase
{
    private const int TriggerDay = 37;
    private const int LowGuardThreshold = 5;
    private const int LowGuardUnrest = 15;

    public BetrayalFromWithinEvent() : base("betrayal_within", "Betrayal from Within",
        "Day 37: Guards defect (scales with guard count). If guards < 5 afterward, +15 unrest.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        var guardsLost = Math.Max(1, state.Population.Guards / 3);
        var defected = Math.Min(state.Population.Guards, guardsLost);
        state.Population.Guards -= defected;
        state.Population.HealthyWorkers += defected;

        report.Add(ReasonTags.Event, $"{Name}: {defected} guards defected. They rejoin as workers.");

        if (state.Population.Guards < LowGuardThreshold)
        {
            StateChangeApplier.AddUnrest(state, LowGuardUnrest, report, ReasonTags.Event, $"{Name} panic");
            report.Add(ReasonTags.Event, $"{Name}: with so few guards remaining, unrest surges.");
        }

        StartCooldown(state);
    }
}
