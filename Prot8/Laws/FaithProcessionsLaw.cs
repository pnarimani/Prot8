using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class FaithProcessionsLaw : LawBase
{
    private const int MoraleGain = 15;
    private const int MaterialsCost = 10;
    private const int UnrestHit = 5;

    public FaithProcessionsLaw() : base("faith_processions", "Faith Processions", "+15 morale, -10 materials, +5 unrest. Requires morale < 40.")
    {
    }

    public override bool CanEnact(GameState state, out string reason)
    {
        if (state.Morale < 40)
        {
            reason = string.Empty;
            return true;
        }

        reason = "Requires morale below 40.";
        return false;
    }

    public override void OnEnact(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddMorale(state, MoraleGain, report, ReasonTags.LawEnact, Name);
        StateChangeApplier.AddResource(state, Resources.ResourceKind.Materials, -MaterialsCost, report, ReasonTags.LawEnact, Name);
        StateChangeApplier.AddUnrest(state, UnrestHit, report, ReasonTags.LawEnact, Name);
    }
}