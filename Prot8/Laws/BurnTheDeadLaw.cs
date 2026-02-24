using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class BurnTheDeadLaw : ILaw
{
    private const int DailySicknessReduction = 4;
    private const int DailyMoraleHit = 2;
    private const int DailyFuelCost = 2;
    private const int MoraleHit = 10;
    private const int SicknessThreshold = 35;

    public string Id => "burn_the_dead";
    public string Name => "Burn the Dead";
    public string GetTooltip(GameState state) => $"-{DailySicknessReduction} sickness/day, -{DailyMoraleHit} morale/day, -{DailyFuelCost} fuel/day, -{MoraleHit} morale on enact. Requires sickness > {SicknessThreshold}.";

    public bool CanEnact(GameState state, out string reason)
    {
        if (state.Sickness > SicknessThreshold)
        {
            reason = string.Empty;
            return true;
        }

        reason = $"Requires sickness above {SicknessThreshold}.";
        return false;
    }

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        state.AddMorale(-MoraleHit, entry);
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        state.AddSickness(-DailySicknessReduction, entry);
        state.AddMorale(-DailyMoraleHit, entry);
        state.AddResource(Resources.ResourceKind.Fuel, -DailyFuelCost, entry);
    }
}
