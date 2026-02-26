using Prot8.Constants;
using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class DistributeLuxuriesOrder : IEmergencyOrder
{
    public string Id => "distribute_luxuries";
    public string Name => "Distribute Luxuries";
    public int CooldownDays => GameBalance.DistributeLuxuriesCooldown;

    public string GetTooltip(GameState state) =>
        $"-{GameBalance.DistributeLuxuriesFuelCost} fuel, -{GameBalance.DistributeLuxuriesMaterialsCost} materials. " +
        $"+{GameBalance.DistributeLuxuriesMoraleGain} morale, -{-GameBalance.DistributeLuxuriesUnrest} unrest, " +
        $"-{-GameBalance.DistributeLuxuriesSickness} sickness. " +
        $"Requires materials >= {GameBalance.DistributeLuxuriesMaterialsGate} and fuel >= {GameBalance.DistributeLuxuriesFuelGate}.";

    public bool CanIssue(GameState state)
    {
        if (!GameBalance.EnableMoraleOrders)
            return false;
        if (state.Resources[ResourceKind.Materials] < GameBalance.DistributeLuxuriesMaterialsGate)
            return false;
        if (state.Resources[ResourceKind.Fuel] < GameBalance.DistributeLuxuriesFuelGate)
            return false;
        return true;
    }

    public void Apply(GameState state, ResolutionEntry entry)
    {
        state.AddResource(ResourceKind.Fuel, -GameBalance.DistributeLuxuriesFuelCost, entry);
        state.AddResource(ResourceKind.Materials, -GameBalance.DistributeLuxuriesMaterialsCost, entry);
        state.AddMorale(GameBalance.DistributeLuxuriesMoraleGain, entry);
        state.AddUnrest(GameBalance.DistributeLuxuriesUnrest, entry);
        state.AddSickness(GameBalance.DistributeLuxuriesSickness, entry);
        entry.Write("Small comforts are distributed: warm blankets, candles, salves. The people remember what it feels like to be cared for.");
    }
}
