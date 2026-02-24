using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class RationMedicineOrder : IEmergencyOrder
{
    private const int MedicineCost = 8;
    private const int SicknessReduction = 15;
    private const int UnrestGain = 5;
    private const int SicknessThreshold = 20;

    public string Id => "ration_medicine";
    public string Name => "Ration the Medicine";
    public int CooldownDays => 3;

    public string GetTooltip(GameState state) =>
        $"-{MedicineCost} medicine, -{SicknessReduction} sickness, +{UnrestGain} unrest. Requires sickness > {SicknessThreshold}.";

    public bool CanIssue(GameState state, out string reason)
    {
        if (state.Sickness <= SicknessThreshold)
        {
            reason = $"Requires sickness above {SicknessThreshold}.";
            return false;
        }

        if (!state.Resources.Has(ResourceKind.Medicine, MedicineCost))
        {
            reason = $"Requires at least {MedicineCost} medicine.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void Apply(GameState state, ResolutionEntry entry)
    {
        state.AddResource(ResourceKind.Medicine, -MedicineCost, entry);
        state.AddSickness(-SicknessReduction, entry);
        state.AddUnrest(UnrestGain, entry);
        entry.Write("Medicine is distributed under armed guard. The sick are triaged — those deemed unlikely to recover get nothing. The city seethes, but the plague recedes.");
    }
}
