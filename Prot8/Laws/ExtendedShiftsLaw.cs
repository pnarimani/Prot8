using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class ExtendedShiftsLaw : ILaw
{
    private const double ProductionMultiplier = 1.25;
    private const int DailySickness = 2;
    private const int DeathChancePercent = 30;
    private const int MoraleHit = 15;
    private const int MinimumDay = 5;

    public string Id => "extended_shifts";
    public string Name => "Extended Shifts";
    public string GetTooltip(GameState state) => $"+{(ProductionMultiplier - 1) * 100}% production, +{DailySickness} sickness/day, {DeathChancePercent}% chance of 1 death/day, -{MoraleHit} morale on enact. Day {MinimumDay}+.";

    public bool CanEnact(GameState state, out string reason)
    {
        if (state.Day >= MinimumDay)
        {
            reason = string.Empty;
            return true;
        }

        reason = $"Available from Day {MinimumDay}.";
        return false;
    }

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        entry.Write("The factories and forges roar through the night. Sleep is a weakness the siege cannot afford.");
        state.AddMorale(-MoraleHit, entry);
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        state.DailyEffects.ProductionMultiplier *= ProductionMultiplier;
        state.AddSickness(DailySickness, entry);
        if (state.RollPercent() <= DeathChancePercent)
        {
            entry.Write("A worker collapses from exhaustion. The shift continues without them.");
            state.ApplyDeath(1, entry);
        }
    }
}
