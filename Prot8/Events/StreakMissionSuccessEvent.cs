using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class StreakMissionSuccessEvent : ITriggeredEvent
{
    public string Id => "streak_mission_success";
    public string Name => "Fortune Favors the Bold";
    public string Description => "Back-to-back successes in the field have emboldened the people. Volunteers step forward.";

    public bool ShouldTrigger(GameState state)
    {
        if (!GameBalance.EnableGoodDayMoraleBoost)
            return false;

        return state.ConsecutiveMissionSuccesses >= GameBalance.StreakMissionSuccessRequired;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        state.Population.HealthyWorkers += GameBalance.StreakMissionSuccessWorkerBonus;
        state.ConsecutiveMissionSuccesses = 0;
        entry.Write($"Inspired by recent victories, {GameBalance.StreakMissionSuccessWorkerBonus} volunteers join the workforce.");
    }
}
