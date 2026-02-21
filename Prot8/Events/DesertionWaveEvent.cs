using Prot8.Simulation;

namespace Prot8.Events;

public sealed class DesertionWaveEvent : TriggeredEventBase
{
    public DesertionWaveEvent() : base("desertion_wave", "Desertion Wave", "Trigger when morale drops below 30.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Morale < 30;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.ApplyDesertions(state, 10, report, ReasonTags.Event, Name);
        StartCooldown(state);
    }
}