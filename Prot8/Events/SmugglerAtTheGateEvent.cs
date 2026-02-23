using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class SmugglerAtTheGateEvent : TriggeredEventBase
{
    private const int TriggerDay = 3;
    private const int FoodGain = 20;
    private const int MaterialsCost = 15;

    public SmugglerAtTheGateEvent() : base("smuggler_gate", "Smuggler at the Gate",
        "Day 3: A smuggler offers food for materials. +20 food, -15 materials (if materials >= 15).")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay && state.Resources.Has(ResourceKind.Materials, MaterialsCost);
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddResource(state, ResourceKind.Food, FoodGain, report, ReasonTags.Event, Name);
        StateChangeApplier.AddResource(state, ResourceKind.Materials, -MaterialsCost, report, ReasonTags.Event, Name);
        report.Add(ReasonTags.Event, $"{Name}: A smuggler slips through the siege lines. Food for materials — a fair trade in desperate times.");
        StartCooldown(state);
    }
}
