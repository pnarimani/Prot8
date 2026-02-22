using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class MandatoryGuardServiceLaw : LawBase
{
    private const int GuardConversion = 10;
    private const int DailyFoodLoss = 15;
    private const int MoraleHit = 10;
    private const int UnrestThreshold = 40;

    public MandatoryGuardServiceLaw() : base("mandatory_guard_service", "Mandatory Guard Service", $"Convert {GuardConversion} workers to guards, -{DailyFoodLoss} food/day, -{MoraleHit} morale. Requires unrest > {UnrestThreshold}.")
    {
    }

    public override string GetDynamicTooltip(GameState state) => $"Convert {GuardConversion} workers to guards, -{DailyFoodLoss} food/day, -{MoraleHit} morale. Requires unrest > {UnrestThreshold}.";

    public override bool CanEnact(GameState state, out string reason)
    {
        if (state.Unrest > 40)
        {
            reason = string.Empty;
            return true;
        }

        reason = "Requires unrest above 40.";
        return false;
    }

    public override void OnEnact(GameState state, DayResolutionReport report)
    {
        var converted = state.Population.ConvertHealthyToGuards(GuardConversion);
        state.RebalanceHousing();
        report.Add(ReasonTags.LawEnact, $"{Name}: converted {converted} healthy workers into guards permanently.");
        StateChangeApplier.AddMorale(state, -MoraleHit, report, ReasonTags.LawEnact, Name);
    }

    public override void ApplyDaily(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddResource(state, Resources.ResourceKind.Food, -DailyFoodLoss, report, ReasonTags.LawPassive, Name);
    }
}