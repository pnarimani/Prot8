using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class SupplyCartsInterceptedEvent : TriggeredEventBase
{
    private const int MinDay = 3;
    private const int MaxDay = 5;
    private const int TriggerChance = 20;
    private const int SupplyLoss = 15;

    public SupplyCartsInterceptedEvent() : base("supply_carts_intercepted", "Supply Carts Intercepted",
        "Days 3-5: 20% daily chance. -15 food or -15 water (random).")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        if (state.Day < MinDay || state.Day > MaxDay)
        {
            return false;
        }

        return state.Random.Next(1, 101) <= TriggerChance;
    }

    public override void Apply(GameState state, ResolutionEntry entry)
    {
        var targetFood = state.RollPercent() <= 50;
        if (targetFood)
        {
            state.AddResource(ResourceKind.Food, -SupplyLoss, entry);
            entry.Write($"{Name}: enemy raiders intercepted a food supply cart.");
        }
        else
        {
            state.AddResource(ResourceKind.Water, -SupplyLoss, entry);
            entry.Write($"{Name}: enemy raiders intercepted a water supply cart.");
        }

        StartCooldown(state);
    }
}
