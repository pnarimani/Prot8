using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class FaithProcessionsLaw : ILaw
{
    private const int MoraleGain = 15;
    private const int MaterialsCost = 10;
    private const int UnrestHit = 5;
    private const int MoraleThreshold = 40;

    public string Id => "faith_processions";
    public string Name => "Fatih Processions";
    public string GetTooltip(GameState state) => $"+{MoraleGain} morale, -{MaterialsCost} materials, +{UnrestHit} unrest. Requires morale < {MoraleThreshold}.";

    public bool CanEnact(GameState state, out string reason)
    {
        if (state.Morale < 40)
        {
            reason = string.Empty;
            return true;
        }

        reason = "Requires morale below 40.";
        return false;
    }

    public void OnEnact(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddMorale(state, MoraleGain, report, ReasonTags.LawEnact, Name);
        StateChangeApplier.AddResource(state, Resources.ResourceKind.Materials, -MaterialsCost, report, ReasonTags.LawEnact, Name);
        StateChangeApplier.AddUnrest(state, UnrestHit, report, ReasonTags.LawEnact, Name);
    }

    public void ApplyDaily(GameState state, DayResolutionReport report)
    {
        
    }
}