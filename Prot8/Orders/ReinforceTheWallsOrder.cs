using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class ReinforceTheWallsOrder : IEmergencyOrder
{
    private const int IntegrityGain = 15;
    private const int MaterialsCost = 15;

    public string Id => "reinforce_walls";
    public string Name => "Reinforce the Walls";
    public int CooldownDays => 3;

    public string GetTooltip(GameState state) =>
        $"+{IntegrityGain} integrity to perimeter, -{MaterialsCost} materials. Requires Fortification >= 2.";

    public bool CanIssue(GameState state, out string reason)
    {
        if (state.Flags.Fortification < 2)
        {
            reason = "Requires fortification commitment.";
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
        state.Flags.Fortification.Add(1, lifetimeDays: 3);
        state.AddResource(ResourceKind.Materials, -MaterialsCost, entry);
        var perimeter = state.ActivePerimeterZone;
        var before = perimeter.Integrity;
        perimeter.Integrity = Math.Min(100, perimeter.Integrity + IntegrityGain);
        var applied = perimeter.Integrity - before;
        entry.Write($"Engineers reinforce the {perimeter.Name} walls. +{applied} integrity. The stone holds firm.");
    }
}
