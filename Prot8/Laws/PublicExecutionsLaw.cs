using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class PublicExecutionsLaw : LawBase
{
    private const int UnrestReduction = 25;
    private const int MoraleHit = 20;
    private const int Deaths = 5;

    public PublicExecutionsLaw() : base("public_executions", "Public Executions", "-25 unrest instantly, -20 morale, 5 deaths. Requires unrest > 60.")
    {
    }

    public override bool CanEnact(GameState state, out string reason)
    {
        if (state.Unrest > 60)
        {
            reason = string.Empty;
            return true;
        }

        reason = "Requires unrest above 60.";
        return false;
    }

    public override void OnEnact(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddUnrest(state, -UnrestReduction, report, ReasonTags.LawEnact, Name);
        StateChangeApplier.AddMorale(state, -MoraleHit, report, ReasonTags.LawEnact, Name);
        StateChangeApplier.ApplyDeaths(state, Deaths, report, ReasonTags.LawEnact, Name);
    }
}