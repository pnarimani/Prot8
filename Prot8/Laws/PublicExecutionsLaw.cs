using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class PublicExecutionsLaw : ILaw
{
    private const int UnrestReduction = 25;
    private const int MoraleHit = 20;
    private const int Deaths = 5;
    private const int UnrestThreshold = 60;

    public string Id => "public_executions";
    public string Name => "Public Executions";
    public string GetTooltip(GameState state) => $"-{UnrestReduction} unrest instantly, -{MoraleHit} morale, {Deaths} deaths. Requires unrest > {UnrestThreshold}.";

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
        state.AddUnrest(-UnrestReduction, entry);
        state.AddMorale(-MoraleHit, entry);
        state.ApplyDeath(Deaths, entry);
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
    }
}