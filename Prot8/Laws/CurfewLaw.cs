using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class CurfewLaw : LawBase
{
    private const int DailyUnrestReduction = 10;
    private const double ProductionMultiplier = 0.8;

    public CurfewLaw() : base("curfew", "Curfew", "-10 unrest/day, -20% production. Requires unrest > 50.")
    {
    }

    public override bool CanEnact(GameState state, out string reason)
    {
        if (state.Unrest > 50)
        {
            reason = string.Empty;
            return true;
        }

        reason = "Requires unrest above 50.";
        return false;
    }

    public override void ApplyDaily(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddUnrest(state, -DailyUnrestReduction, report, ReasonTags.LawPassive, Name);
        state.DailyEffects.ProductionMultiplier *= ProductionMultiplier;
    }
}