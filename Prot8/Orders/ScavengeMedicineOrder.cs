using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class ScavengeMedicineOrder : IEmergencyOrder
{
    private const int MedicineGain = 12;
    private const int SicknessHit = 5;
    private const int Deaths = 1;

    public string Id => "scavenge_medicine";
    public string Name => "Scavenge Medicine";
    public int CooldownDays => 3;

    public string GetTooltip(GameState state) =>
        $"+{MedicineGain} medicine, +{SicknessHit} sickness from exposure, {Deaths} death from scavenging.";

    public bool CanIssue(GameState state, out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public void Apply(GameState state, ResolutionEntry entry)
    {
        state.AddResource(ResourceKind.Medicine, MedicineGain, entry);
        state.AddSickness(SicknessHit, entry);
        state.ApplyDeath(Deaths, entry);
        entry.Write("Volunteers scavenge through ruins and abandoned buildings. They return with medicine, but exposed to disease. One does not return.");
    }
}
