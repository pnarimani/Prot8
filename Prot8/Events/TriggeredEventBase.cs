using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Events;

public static class EventExtensions
{
    extension(ITriggeredEvent evt)
    {
        public bool IsOnCooldown(GameState state)
        {
            return state.EventCooldowns.TryGetValue(evt.Id, out var remaining) && remaining > 0;
        }

        public void StartCooldown(GameState state)
        {
            if (GameBalance.EventCooldownDays.TryGetValue(evt.Id, out var days) && days > 0)
            {
                state.EventCooldowns[evt.Id] = days;
            }
        }
    }
}