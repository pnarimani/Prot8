using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class SewerCleanupOrder : IEmergencyOrder
{
    private const int SicknessReduction = 4;
    private const int FuelCost = 3;

    public string Id => "sewer_cleanup";
    public string Name => "Sewer Cleanup";
    public int CooldownDays => 2;

    public string GetTooltip(GameState state) =>
        $"-{SicknessReduction} sickness, -{FuelCost} fuel. Requires Plague Rats active.";

    public bool CanIssue(GameState state, out string reason)
    {
        if (!state.PlagueRatsActive)
        {
            reason = "Requires Plague Rats to be active.";
            return false;
        }

        if (!state.Resources.Has(ResourceKind.Fuel, FuelCost))
        {
            reason = $"Requires at least {FuelCost} fuel.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void Apply(GameState state, ResolutionEntry entry)
    {
        state.AddResource(ResourceKind.Fuel, -FuelCost, entry);
        state.AddSickness(-SicknessReduction, entry);
        entry.Write("Workers descend into the sewers with torches and disinfectant. The worst of the filth is cleared. Sickness recedes, but fuel is consumed.");
    }
}
