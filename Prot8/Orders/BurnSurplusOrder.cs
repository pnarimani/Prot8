using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class BurnSurplusOrder : IEmergencyOrder
{
    const int MaterialsCost = 5;
    const int SicknessReduction = 5;
    const int MoraleGain = 10;

    public string Id => "burn_surplus";
    public string Name => "Burn Surplus for Warmth";
    public int CooldownDays => 2;

    public string GetTooltip(GameState state)
    {
        return $"-{MaterialsCost} materials, -{SicknessReduction} sickness, +{MoraleGain} morale.";
    }

    public bool CanIssue(GameState state, out string reason)
    {
        if (!state.Resources.Has(ResourceKind.Materials, MaterialsCost))
        {
            reason = $"Requires at least {MaterialsCost} materials.";
            return false;
        }

        if (state.Day < 10)
        {
            reason = "";
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
    }
}