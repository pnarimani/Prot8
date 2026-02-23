using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class MandatoryGuardServiceLaw : ILaw
{
    const int GuardConversion = 10;
    const int DailyFoodLoss = 15;
    const int MoraleHit = 10;
    const int UnrestThreshold = 40;

    public string Id => "mandatory_guard_service";
    public string Name => "Mandatory Guard Service";

    public string GetTooltip(GameState state) =>
        $"Convert {Math.Min(GuardConversion, state.Population.HealthyWorkers)}/{GuardConversion} workers to guards, -{DailyFoodLoss} food/day, -{MoraleHit} morale. Requires unrest > {UnrestThreshold}.";

    public bool CanEnact(GameState state, out string reason)
    {
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
        var converted = state.Population.ConvertHealthyToGuards(GuardConversion);
        report.Add(ReasonTags.LawEnact, $"{Name}: converted {converted} healthy workers into guards permanently.");
        StateChangeApplier.AddMorale(state, -MoraleHit, report, ReasonTags.LawEnact, Name);
    }

    public void ApplyDaily(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddResource(state, ResourceKind.Food, -DailyFoodLoss, report, ReasonTags.LawPassive, Name);
    }
}