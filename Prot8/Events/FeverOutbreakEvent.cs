using Prot8.Simulation;

namespace Prot8.Events;

public sealed class FeverOutbreakEvent() : ITriggeredEvent
{
    public string Id => "fever_outbreak";
    public string Name => "Fever Outbreak";
    public string Description => "Sickness has reached a breaking point. A sweating fever tears through the overcrowded quarters, filling the clinic past capacity and killing those already weak.";

    const int SicknessThreshold = 60;
    const int Deaths = 10;
    const int UnrestGain = 10;

    public bool ShouldTrigger(GameState state)
    {
        return state.Sickness > SicknessThreshold;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        entry.Write("The fever clinic overflows. " +
                    "Carts carry the dead at dawn. " +
                    "The outbreak claims lives and rattles the nerves of those still standing.");
        state.ApplyDeath(Deaths, entry);
        state.AddUnrest(UnrestGain, entry);
    }
}