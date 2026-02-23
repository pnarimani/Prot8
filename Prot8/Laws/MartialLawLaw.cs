using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class MartialLawLaw : ILaw
{
    private const int UnrestCap = 60;
    private const int MoraleCap = 35;
    private const int UnrestThreshold = 75;
    private const int DailyDeaths = 2;
    private const int DailyFoodCost = 10;

    public string Id => "martial_law";
    public string Name => "Martial Law";

    public string GetTooltip(GameState state) => $"Unrest cannot exceed {UnrestCap}, morale capped at {MoraleCap}. {DailyDeaths} executions/day, -{DailyFoodCost} food/day. Requires unrest > {UnrestThreshold}. Incompatible with Curfew.";

    public bool CanEnact(GameState state, out string reason)
    {
        if (state.ActiveLawIds.Contains("curfew"))
        {
            reason = "Incompatible with Curfew.";
            return false;
        }

        if (state.Unrest > UnrestThreshold)
        {
            reason = string.Empty;
            return true;
        }

        reason = $"Requires unrest above {UnrestThreshold}.";
        return false;
    }

    public void OnEnact(GameState state, DayResolutionReport report)
    {
    }

    public void ApplyDaily(GameState state, DayResolutionReport report)
    {
        if (state.Unrest > UnrestCap)
        {
            var reduction = state.Unrest - UnrestCap;
            StateChangeApplier.AddUnrest(state, -reduction, report, ReasonTags.LawPassive, Name);
        }

        if (state.Morale > MoraleCap)
        {
            var reduction = state.Morale - MoraleCap;
            StateChangeApplier.AddMorale(state, -reduction, report, ReasonTags.LawPassive, Name);
        }

        StateChangeApplier.ApplyDeaths(state, DailyDeaths, report, ReasonTags.LawPassive, $"{Name} enforcement");
        StateChangeApplier.AddResource(state, ResourceKind.Food, -DailyFoodCost, report, ReasonTags.LawPassive, $"{Name} patrols");
    }
}
