using Prot8.Simulation;

namespace Prot8.Events;

public sealed class NarrativeSiegeBeatEvent : TriggeredEventBase
{
    private readonly int _triggerDay;
    private readonly string _narrativeText;

    public NarrativeSiegeBeatEvent(string id, string name, int triggerDay, string narrativeText)
        : base(id, name, $"Narrative event on day {triggerDay}.")
    {
        _triggerDay = triggerDay;
        _narrativeText = narrativeText;
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Day == _triggerDay;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        report.Add(ReasonTags.Event, _narrativeText);
        StartCooldown(state);
    }
}
