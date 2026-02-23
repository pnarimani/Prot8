using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class HungerRiotEvent : TriggeredEventBase
{
    private const int ConsecutiveDeficitDays = 2;
    private const int UnrestThreshold = 50;
    private const int FoodLost = 80;
    private const int Deaths = 5;
    private const int UnrestGain = 15;

    public HungerRiotEvent() : base("hunger_riot", "Hunger Riot",
        $"Food deficit for {ConsecutiveDeficitDays} consecutive days and unrest > {UnrestThreshold}. -{FoodLost} food, {Deaths} deaths, +{UnrestGain} unrest.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.ConsecutiveFoodDeficitDays >= ConsecutiveDeficitDays && state.Unrest > UnrestThreshold;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddResource(state, ResourceKind.Food, -FoodLost, report, ReasonTags.Event, Name);
        StateChangeApplier.ApplyDeaths(state, Deaths, report, ReasonTags.Event, Name);
        StateChangeApplier.AddUnrest(state, UnrestGain, report, ReasonTags.Event, Name);
        StartCooldown(state);
    }
}