using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class StreakLowSicknessEvent : ITriggeredEvent
{
    public string Id => "streak_low_sickness";
    public string Name => "Health Improving";
    public string Description => "The clinics report fewer patients each day. The plague's grip is weakening.";

    public bool ShouldTrigger(GameState state)
    {
        if (!GameBalance.EnableGoodDayMoraleBoost)
            return false;

        return state.ConsecutiveLowSicknessDays >= GameBalance.StreakLowSicknessDaysRequired;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        state.AddMorale(GameBalance.StreakLowSicknessMoraleBoost, entry);
        state.AddUnrest(-GameBalance.StreakLowSicknessUnrestReduction, entry);
        state.ConsecutiveLowSicknessDays = 0;
        entry.Write("Days of low sickness have restored confidence. The healers breathe easier, and the people with them.");
    }
}
