using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class FortifyTheGateOrder : IEmergencyOrder
{
    private const int MaterialsCost = 8;
    private const int IntegrityGain = 10;
    private const int UnrestGain = 3;
    private const int IntegrityThreshold = 70;

    public string Id => "fortify_gate";
    public string Name => "Fortify the Gate";
    public int CooldownDays => 3;

    public string GetTooltip(GameState state) =>
        $"-{MaterialsCost} materials, +{IntegrityGain} integrity, +{UnrestGain} unrest. Requires perimeter integrity < {IntegrityThreshold}.";

    public bool CanIssue(GameState state, out string reason)
    {
        if (state.ActivePerimeterZone.Integrity >= IntegrityThreshold)
        {
            reason = $"Requires perimeter integrity below {IntegrityThreshold}.";
            return false;
        }

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
        var perimeter = state.ActivePerimeterZone;
        var before = perimeter.Integrity;
        perimeter.Integrity = Math.Min(100, perimeter.Integrity + IntegrityGain);
        var applied = perimeter.Integrity - before;
        state.AddUnrest(UnrestGain, entry);
        entry.Write($"Workers are pulled from their posts and forced to shore up the {perimeter.Name}. The walls hold firmer — but the people resent the forced labor.");
    }
}
