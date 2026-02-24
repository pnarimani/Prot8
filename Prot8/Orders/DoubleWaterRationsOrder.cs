using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class DoubleWaterRationsOrder : IEmergencyOrder
{
    private const int MoraleGain = 5;
    private const int WaterCost = 15;

    public string Id => "double_water_rations";
    public string Name => "Double Water Rations";
    public int CooldownDays => 2;

    public string GetTooltip(GameState state) =>
        $"+{MoraleGain} morale, -{WaterCost} water.";

    public bool CanIssue(GameState state, out string reason)
    {
        if (!state.Resources.Has(ResourceKind.Water, WaterCost))
        {
            reason = $"Requires at least {WaterCost} water.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void Apply(GameState state, ResolutionEntry entry)
    {
        state.AddMorale(MoraleGain, entry);
        state.AddResource(ResourceKind.Water, -WaterCost, entry);
    }
}
