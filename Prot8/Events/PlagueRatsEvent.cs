using Prot8.Simulation;

namespace Prot8.Events;

public sealed class PlagueRatsEvent : TriggeredEventBase
{
    private const int TriggerDay = 18;
    private const int SicknessGain = 15;
    private const int Deaths = 3;
    private const int UnrestGain = 10;

    public PlagueRatsEvent() : base("plague_rats", "Plague Rats",
        "Triggers on day 18. +15 sickness, 3 deaths, +10 unrest. Permanently increases sickness growth.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddSickness(state, SicknessGain, report, ReasonTags.Event, Name);
        StateChangeApplier.ApplyDeaths(state, Deaths, report, ReasonTags.Event, Name);
        StateChangeApplier.AddUnrest(state, UnrestGain, report, ReasonTags.Event, $"{Name} panic");

        state.PlagueRatsActive = true;
        report.Add(ReasonTags.Event, $"{Name}: rats carry disease into the inner city. Sickness will spread faster from now on.");
        StartCooldown(state);
    }
}
