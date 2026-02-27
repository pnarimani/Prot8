using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class StreakZoneHeldEvent : ITriggeredEvent
{
    public string Id => "streak_zone_held";
    public string Name => "Walls Still Stand";
    public string Description => "Against the siege's fury, the perimeter holds. The defenders take pride in their resilience.";

    public bool ShouldTrigger(GameState state)
    {
        if (!GameBalance.EnableGoodDayMoraleBoost)
            return false;

        return state.ConsecutiveZoneHeldDays >= GameBalance.StreakZoneHeldDaysRequired;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        state.AddMorale(GameBalance.StreakZoneHeldMoraleBoost, entry);
        state.ConsecutiveZoneHeldDays = 0;
        entry.Write("The walls hold firm. The garrison's determination inspires the whole city — we can endure this.");
    }
}
