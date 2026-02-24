using Prot8.Simulation;

namespace Prot8.Events;

public sealed class FinalAssaultEvent() : ITriggeredEvent
{
    public string Id => "final_assault";
    public string Name => "Final Assault Begins";
    public string Description => "The enemy has marshaled their full strength for a final push. Every siege engine, every soldier, all of it aimed at your gates.";

    const int TriggerDay = 33;
    const int UnrestGain = 15;
    const int MoraleLoss = 10;

    public bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        state.FinalAssaultActive = true;
        entry.Write("Battering rams thunder against the gates. Ladders rise against the walls. The enemy no longer waits, they come for you now.");
        state.AddUnrest(UnrestGain, entry);
    }
}