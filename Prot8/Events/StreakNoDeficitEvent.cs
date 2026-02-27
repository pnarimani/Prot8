using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class StreakNoDeficitEvent : ITriggeredEvent
{
    public string Id => "streak_no_deficit";
    public string Name => "Steady Supplies";
    public string Description => "For days now, the granary and cisterns have held. The people notice — and take heart.";

    public bool ShouldTrigger(GameState state)
    {
        if (!GameBalance.EnableGoodDayMoraleBoost)
            return false;

        return state.ConsecutiveNoDeficitDays >= GameBalance.StreakNoDeficitDaysRequired;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        state.AddMorale(GameBalance.StreakNoDeficitMoraleBoost, entry);
        state.ConsecutiveNoDeficitDays = 0;
        entry.Write("The steady flow of food and water lifts spirits across the city. Perhaps things are looking up.");
    }
}
