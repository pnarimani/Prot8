using Prot8.Simulation;

namespace Prot8.Events;

public sealed class FeverOutbreakEvent : TriggeredEventBase
{
    public FeverOutbreakEvent() : base("fever_outbreak", "Fever Outbreak", "Trigger when sickness exceeds 60.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Sickness > 60;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.ApplyDeaths(state, 10, report, ReasonTags.Event, Name);
        StateChangeApplier.AddUnrest(state, 10, report, ReasonTags.Event, Name);
        StartCooldown(state);
    }
}