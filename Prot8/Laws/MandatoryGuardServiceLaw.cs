using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class MandatoryGuardServiceLaw : ILaw
{
    const int GuardConversion = 5;
    const int DailyFoodLoss = 4;
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

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        state.Flags.Fortification.Add(1);
        var converted = state.Population.ConvertHealthyToGuards(GuardConversion);
        entry.Write($"Workers are conscripted into the garrison. {converted} take up arms, leaving their trades behind.");
        state.AddMorale(-MoraleHit, entry);
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        state.AddResource(ResourceKind.Food, -DailyFoodLoss, entry);
        entry.Write("Extra mouths feed at the garrison table.");
    }
}
