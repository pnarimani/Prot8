using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class ScavengeMedicineOrder : IEmergencyOrder
{
    private const int MedicineGain = 20;
    private const int SicknessHit = 5;
    private const int Deaths = 2;
    private const int MedicineThreshold = 15;

    public string Id => "scavenge_medicine";
    public string Name => "Scavenge Medicine";
    public int CooldownDays => 3;

    public string GetTooltip(GameState state) =>
        $"+{MedicineGain} medicine, +{SicknessHit} sickness, {Deaths} deaths. Requires medicine < {MedicineThreshold}.";

    public bool CanIssue(GameState state, out string reason)
    {
        if (state.Resources[ResourceKind.Medicine] >= MedicineThreshold)
        {
            reason = $"Requires medicine below {MedicineThreshold}.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void Apply(GameState state, ResolutionEntry entry)
    {
        state.AddResource(ResourceKind.Medicine, MedicineGain, entry);
        state.AddSickness(SicknessHit, entry);
        state.ApplyDeath(Deaths, entry);
        entry.Write("Volunteers are sent beyond the walls to scavenge abandoned apothecaries. They return with armfuls of medicine — but two do not return at all.");
    }
}
