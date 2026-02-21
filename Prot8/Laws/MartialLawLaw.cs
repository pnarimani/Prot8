using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class MartialLawLaw : LawBase
{
    private const int UnrestCap = 60;
    private const int MoraleCap = 40;

    public MartialLawLaw() : base("martial_law", "Martial Law", "Unrest cannot exceed 60, morale capped at 40. Requires unrest > 75.")
    {
    }

    public override bool CanEnact(GameState state, out string reason)
    {
        if (state.Unrest > 75)
        {
            reason = string.Empty;
            return true;
        }

        reason = "Requires unrest above 75.";
        return false;
    }

    public override void ApplyDaily(GameState state, DayResolutionReport report)
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
    }
}