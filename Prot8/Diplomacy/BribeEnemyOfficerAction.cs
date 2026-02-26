using Prot8.Constants;
using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Diplomacy;

public sealed class BribeEnemyOfficerAction : IDiplomaticAction
{
    public string Id => "bribe_enemy_officer";
    public string Name => "Bribe Enemy Officer";
    public bool CanDeactivate => true;

    public string GetTooltip(GameState state)
    {
        var foodCost = state.Flags.Tyranny >= 3 ? GameBalance.BribeFoodCostTyranny : GameBalance.BribeFoodCost;
        var matCost = state.Flags.Tyranny >= 3 ? GameBalance.BribeMaterialsCostTyranny : GameBalance.BribeMaterialsCost;
        return $"Daily: -{foodCost} food, -{matCost} materials. -20% siege damage, 5% daily interception risk (+10 unrest).";
    }

    public bool CanActivate(GameState state) => true;

    public void OnActivate(GameState state, ResolutionEntry entry)
    {
        entry.Write("A bag of coin and provisions is smuggled to an enemy officer. The siege eases... for now.");
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        var foodCost = state.Flags.Tyranny >= 3 ? GameBalance.BribeFoodCostTyranny : GameBalance.BribeFoodCost;
        var matCost = state.Flags.Tyranny >= 3 ? GameBalance.BribeMaterialsCostTyranny : GameBalance.BribeMaterialsCost;

        state.AddResource(ResourceKind.Food, -foodCost, entry);
        state.AddResource(ResourceKind.Materials, -matCost, entry);

        state.DailyEffects.SiegeDamageMultiplier = GameBalance.BribeSiegeDamageMultiplier;

        if (state.RollPercent() <= GameBalance.BribeInterceptionChance)
        {
            state.AddUnrest(GameBalance.BribeInterceptionUnrest, entry);
            entry.Write("The bribery was discovered! Enemy officers rage. Unrest spreads through the city.");
        }
    }
}
