using Prot8.Simulation;

namespace Prot8.Events;

public sealed class MilitiaVolunteersEvent : TriggeredEventBase
{
    private const int TriggerDay = 6;
    private const int GuardsGain = 3;
    private const int WorkersLost = 3;

    public MilitiaVolunteersEvent() : base("militia_volunteers", "Militia Volunteers",
        "Day 6: Workers volunteer for guard duty. +3 guards, -3 healthy workers.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay && state.Population.HealthyWorkers >= WorkersLost;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        var converted = state.Population.ConvertHealthyToGuards(WorkersLost);
        state.Allocation.RemoveWorkersProportionally(converted);
        report.Add(ReasonTags.Event, $"{Name}: {converted} workers take up arms. \"We'd rather fight than starve behind walls.\"");
        StartCooldown(state);
    }
}
