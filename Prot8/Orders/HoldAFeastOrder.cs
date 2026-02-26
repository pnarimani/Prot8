using Prot8.Constants;
using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class HoldAFeastOrder : IEmergencyOrder
{
    public string Id => "hold_a_feast";
    public string Name => "Hold a Feast";
    public int CooldownDays => GameBalance.HoldAFeastCooldown;

    public string GetTooltip(GameState state) =>
        $"-{GameBalance.HoldAFeastFoodCost} food, -{GameBalance.HoldAFeastFuelCost} fuel, " +
        $"+{GameBalance.HoldAFeastMoraleGain} morale, -{-GameBalance.HoldAFeastUnrest} unrest. " +
        $"Requires food >= {GameBalance.HoldAFeastFoodGate}.";

    public bool CanIssue(GameState state)
    {
        if (!GameBalance.EnableMoraleOrders)
            return false;
        if (!state.Resources.Has(ResourceKind.Food, GameBalance.HoldAFeastFoodCost))
            return false;
        if (!state.Resources.Has(ResourceKind.Fuel, GameBalance.HoldAFeastFuelCost))
            return false;
        if (state.Resources[ResourceKind.Food] < GameBalance.HoldAFeastFoodGate)
            return false;
        return true;
    }

    public void Apply(GameState state, ResolutionEntry entry)
    {
        state.AddResource(ResourceKind.Food, -GameBalance.HoldAFeastFoodCost, entry);
        state.AddResource(ResourceKind.Fuel, -GameBalance.HoldAFeastFuelCost, entry);
        state.AddMorale(GameBalance.HoldAFeastMoraleGain, entry);
        state.AddUnrest(GameBalance.HoldAFeastUnrest, entry);
        entry.Write("Tables are laid in the square. For one evening, the siege feels far away. Laughter echoes off stone walls.");
    }
}
