using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class ForcedLaborOrder : IEmergencyOrder
{
    private const int MaterialsGain = 15;
    private const int UnrestGain = 8;
    private const int Deaths = 2;

    public string Id => "forced_labor";
    public string Name => "Forced Labor Detail";
    public int CooldownDays => 3;

    public string GetTooltip(GameState state) =>
        $"+{MaterialsGain} materials, +{UnrestGain} unrest, {Deaths} deaths.";

    public bool CanIssue(GameState state, out string reason)
    {
        if (state.Flags.Faith >= 4)
        {
            reason = "The faithful refuse forced servitude.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void Apply(GameState state, ResolutionEntry entry)
    {
        state.Flags.Tyranny.Add(1);
        state.AddResource(ResourceKind.Materials, MaterialsGain, entry);
        state.AddUnrest(UnrestGain, entry);
        state.ApplyDeath(Deaths, entry);
        entry.Write("Conscripts are driven into the quarries at spearpoint. Materials pile up — but two collapse from exhaustion and do not rise. The others stare with hollow eyes.");
    }
}
