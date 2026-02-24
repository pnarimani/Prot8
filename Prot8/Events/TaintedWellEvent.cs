using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class TaintedWellEvent : TriggeredEventBase
{
    private const int TriggerDay = 20;
    private const int WaterLost = 20;
    private const int SicknessGain = 10;
    private const double WaterProductionPenalty = 0.6;
    private const int PenaltyDuration = 3;

    public TaintedWellEvent() : base("tainted_well", "Tainted Well",
        "Day 20: -20 water, +10 sickness. Water production at 60% for 3 days.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay;
    }

    public override void Apply(GameState state, ResolutionEntry entry)
    {
        state.AddResource(ResourceKind.Water, -WaterLost, entry);
        state.AddSickness(SicknessGain, entry);

        state.TaintedWellDaysRemaining = PenaltyDuration;

        entry.Write($"{Name}: the main well is contaminated. Water production reduced to {WaterProductionPenalty * 100}% for {PenaltyDuration} days.");
        StartCooldown(state);
    }
}
