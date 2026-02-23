using Prot8.Simulation;

namespace Prot8.Events;

public sealed class DesertionWaveEvent : TriggeredEventBase
{
    private const int MoraleThreshold = 30;
    private const int Desertions = 10;

    public DesertionWaveEvent() : base("desertion_wave", "Desertion Wave",
        $"Triggers when morale < {MoraleThreshold}. {Desertions} desertions.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Morale < MoraleThreshold;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        report.Add(ReasonTags.Event, "At dawn, the western gate stands open. Footprints lead into the fog. They chose the enemy over you.");
        StateChangeApplier.ApplyDesertions(state, Desertions, report, ReasonTags.Event, Name);
        StartCooldown(state);
    }
}