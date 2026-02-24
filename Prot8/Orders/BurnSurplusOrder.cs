using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class BurnSurplusOrder : IEmergencyOrder
{
    const int MaterialsCost = 10;
    const int SicknessReduction = 8;
    const int MoraleGain = 8;

    public string Id => "burn_surplus";
    public string Name => "Burn Surplus for Warmth";
    public int CooldownDays => 3;

    public string GetTooltip(GameState state) =>
        $"-{MaterialsCost} materials, -{SicknessReduction} sickness, +{MoraleGain} morale.";

    public bool CanIssue(GameState state, out string reason)
    {
        if (!state.Resources.Has(ResourceKind.Materials, MaterialsCost))
        {
            reason = $"Requires at least {MaterialsCost} materials.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void Apply(GameState state, ResolutionEntry entry)
    {
        state.AddResource(ResourceKind.Materials, -MaterialsCost, entry);
        state.AddSickness(-SicknessReduction, entry);
        state.AddMorale(MoraleGain, entry);
        entry.Write("Surplus materials are fed to the bonfires. The flames push back the cold and lift spirits — but those materials won't be there when the walls need mending.");
    }
}
