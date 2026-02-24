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

    public override void Apply(GameState state, ResolutionEntry entry)
    {
        entry.Write("The granary is ransacked by a mob. Guards are overwhelmed. Screaming from the lower quarter.");
        state.AddResource(ResourceKind.Food, -FoodLost, entry);
        state.ApplyDeath(Deaths, entry);
        state.AddUnrest(UnrestGain, entry);
        StartCooldown(state);
    }
}