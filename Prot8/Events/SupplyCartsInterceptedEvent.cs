using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class SupplyCartsInterceptedEvent()
    : ITriggeredEvent
{
    public string Id => "supply_carts_intercepted";
    public string Name => "Supply Carts Intercepted";
    public string Description => "Enemy cavalry cut off a supply run at the eastern road. The carts were seized before they reached the gates — their contents lost to the siege.";

    private const int MinDay = 3;
    private const int MaxDay = 5;
    private const int TriggerChance = 20;
    private const int SupplyLoss = 15;

    public bool ShouldTrigger(GameState state)
    {
        if (state.Day < MinDay || state.Day > MaxDay)
        {
            return false;
        }

        return state.Random.Next(1, 101) <= TriggerChance;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        var targetFood = state.RollPercent() <= 50;
        if (targetFood)
        {
            entry.Write("Enemy riders cut down the supply convoy at the eastern road. The food carts are seized before they reach the gates. Hungry mouths will go unfed tonight.");
            state.AddResource(ResourceKind.Food, -SupplyLoss, entry);
        }
        else
        {
            entry.Write("Enemy riders intercept the water wagons before they reach the gates. The barrels are smashed and the convoy scattered. The city grows thirstier.");
            state.AddResource(ResourceKind.Water, -SupplyLoss, entry);
        }
    }
}
