using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class FortifyTheGateOrder : IEmergencyOrder
{
    private const int MaterialsCost = 5;
    private const int IntegrityGain = 3;

    public string Id => "fortify_gate";
    public string Name => "Fortify the Gate";
    public int CooldownDays => 2;

    public string GetTooltip(GameState state) =>
        $"-{MaterialsCost} materials, +{IntegrityGain} integrity to perimeter zone.";

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
        var perimeter = state.ActivePerimeterZone;
        var before = perimeter.Integrity;
        perimeter.Integrity = Math.Min(100, perimeter.Integrity + IntegrityGain);
        var applied = perimeter.Integrity - before;
        entry.Write($"{Name}: +{applied} integrity to {perimeter.Name}.");
    }
}
