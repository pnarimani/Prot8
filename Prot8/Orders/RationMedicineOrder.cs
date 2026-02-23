using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class RationMedicineOrder : IEmergencyOrder
{
    private const int MedicineCost = 3;
    private const int SicknessReduction = 3;
    private const int UnrestGain = 2;

    public string Id => "ration_medicine";
    public string Name => "Ration the Medicine";
    public int CooldownDays => 2;

    public string GetTooltip(GameState state) =>
        $"-{MedicineCost} medicine, -{SicknessReduction} sickness, +{UnrestGain} unrest.";

    public bool CanIssue(GameState state, out string reason)
    {
        if (!state.Resources.Has(ResourceKind.Medicine, MedicineCost))
        {
            reason = $"Requires at least {MedicineCost} medicine.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void Apply(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddResource(state, ResourceKind.Medicine, -MedicineCost, report, ReasonTags.OrderEffect, Name);
        StateChangeApplier.AddSickness(state, -SicknessReduction, report, ReasonTags.OrderEffect, Name);
        StateChangeApplier.AddUnrest(state, UnrestGain, report, ReasonTags.OrderEffect, Name);
    }
}
