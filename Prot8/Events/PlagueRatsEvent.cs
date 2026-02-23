using Prot8.Simulation;

namespace Prot8.Events;

public sealed class PlagueRatsEvent : TriggeredEventBase
{
    public PlagueRatsEvent() : base("plague_rats", "Plague Rats",
        "Triggers once on day 22. Permanently increases base sickness growth.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Day == 22;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddSickness(state, 15, report, ReasonTags.Event, Name);
        StateChangeApplier.ApplyDeaths(state, 3, report, ReasonTags.Event, Name);
        StateChangeApplier.AddUnrest(state, 10, report, ReasonTags.Event, $"{Name} panic");

        state.PlagueRatsActive = true;
        report.Add(ReasonTags.Event, $"{Name}: rats carry disease into the inner city. Sickness will spread faster from now on.");
    }
}
