using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class MartialLawLaw : ILaw
{
    private const int UnrestCap = 60;
    private const int MoraleCap = 40;
    private const int UnrestThreshold = 75;

    public string Id => "martial_law";
    public string Name => "Martial Law";
    
    public string GetTooltip(GameState state) => $"Unrest cannot exceed {UnrestCap}, morale capped at {MoraleCap}. Requires unrest > {UnrestThreshold}.";

    public bool CanEnact(GameState state, out string reason)
    {
        if (state.Unrest > 75)
        {
            reason = string.Empty;
            return true;
        }

        reason = "Requires unrest above 75.";
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
    }
}