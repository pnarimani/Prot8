using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class HungerRiotEvent() : ITriggeredEvent
{
    public string Id => "hunger_riot";
    public string Name => "Hunger Riot";
    public string Description => "Days of food shortage have broken the people's patience. A mob has stormed the granaries, killing guards and taking what little remains.";

    const int ConsecutiveDeficitDays = 2;
    const int UnrestThreshold = 50;
    const int FoodLost = 80;
    const int Deaths = 5;
    const int UnrestGain = 15;

    public bool ShouldTrigger(GameState state)
    {
        return state.ConsecutiveFoodDeficitDays >= ConsecutiveDeficitDays && state.Unrest > UnrestThreshold;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        entry.Write("Angry mobs force the granary doors. Guards are beaten back and supplies ransacked. By the time order is restored, lives are lost and stores are depleted.");
        state.AddResource(ResourceKind.Food, -FoodLost, entry);
        state.AddUnrest(UnrestGain, entry);
        state.ApplyGuardDeath(Deaths, entry);
    }
}