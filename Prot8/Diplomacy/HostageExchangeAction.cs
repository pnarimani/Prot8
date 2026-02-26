using Prot8.Constants;
using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Diplomacy;

public sealed class HostageExchangeAction : IDiplomaticAction
{
    public string Id => "hostage_exchange";
    public string Name => "Hostage Exchange";
    public bool CanDeactivate => true;

    public string GetTooltip(GameState state) =>
        $"Daily: -{GameBalance.HostageFoodCost} food, -{GameBalance.HostageMedicineCost} medicine. " +
        $"+1 worker every 2 days, -{GameBalance.HostageDailyMorale} morale/day. Requires at least 1 lost zone.";

    public bool CanActivate(GameState state) => state.CountLostZones() >= 1;

    public void OnActivate(GameState state, ResolutionEntry entry)
    {
        state.HostageExchangeDayCounter = 0;
        entry.Write("Negotiations begin under a white flag. Hostages will be exchanged for supplies.");
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        state.AddResource(ResourceKind.Food, -GameBalance.HostageFoodCost, entry);
        state.AddResource(ResourceKind.Medicine, -GameBalance.HostageMedicineCost, entry);
        state.AddMorale(GameBalance.HostageDailyMorale, entry);

        state.HostageExchangeDayCounter++;
        if (state.HostageExchangeDayCounter >= 2)
        {
            state.HostageExchangeDayCounter = 0;
            state.Population.HealthyWorkers += 1;
            entry.Write("A hostage has been returned. +1 worker.");
        }
    }
}
