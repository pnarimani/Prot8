using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class CurfewLaw : ILaw
{
    private const int DailyUnrestReduction = 10;
    private const double ProductionMultiplier = 0.8;
    private const int UnrestThreshold = 50;

    public string Id => "curfew";
    public string Name => "Curfew";
    public string GetTooltip(GameState state) => $"-{DailyUnrestReduction} unrest/day, -{ProductionMultiplier * 100}% production. Requires unrest > {UnrestThreshold}.";

    public bool CanEnact(GameState state, out string reason)
    {
        if (state.Unrest > 50)
        {
            reason = string.Empty;
            return true;
        }

        reason = "Requires unrest above 50.";
        return false;
    }

    public void OnEnact(GameState state, DayResolutionReport report)
    {
        
    }

    public void ApplyDaily(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddUnrest(state, -DailyUnrestReduction, report, ReasonTags.LawPassive, Name);
        state.DailyEffects.ProductionMultiplier *= ProductionMultiplier;
    }
}