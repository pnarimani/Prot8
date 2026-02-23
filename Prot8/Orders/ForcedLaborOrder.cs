using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class ForcedLaborOrder : IEmergencyOrder
{
    private const int MaterialsGain = 8;
    private const int UnrestGain = 5;
    private const int Deaths = 1;

    public string Id => "forced_labor";
    public string Name => "Forced Labor Detail";
    public int CooldownDays => 2;

    public string GetTooltip(GameState state) =>
        $"+{MaterialsGain} materials, +{UnrestGain} unrest, {Deaths} death.";

    public bool CanIssue(GameState state, out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public void Apply(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddResource(state, ResourceKind.Materials, MaterialsGain, report, ReasonTags.OrderEffect, Name);
        StateChangeApplier.AddUnrest(state, UnrestGain, report, ReasonTags.OrderEffect, Name);
        StateChangeApplier.ApplyDeaths(state, Deaths, report, ReasonTags.OrderEffect, $"{Name} exhaustion");
    }
}
