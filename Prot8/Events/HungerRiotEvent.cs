using Prot8.Simulation;

namespace Prot8.Events;

public sealed class HungerRiotEvent : TriggeredEventBase
{
    public HungerRiotEvent() : base("hunger_riot", "Hunger Riot", "Food deficit for 2 consecutive days and unrest > 50.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.ConsecutiveFoodDeficitDays >= 2 && state.Unrest > 50;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddResource(state, Resources.ResourceKind.Food, -80, report, ReasonTags.Event, Name);
        StateChangeApplier.ApplyDeaths(state, 5, report, ReasonTags.Event, Name);
        StateChangeApplier.AddUnrest(state, 15, report, ReasonTags.Event, Name);
        StartCooldown(state);
    }
}