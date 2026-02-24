using Prot8.Simulation;

namespace Prot8.Events;

public sealed class DesertionWaveEvent : ITriggeredEvent
{
    public string Id => "desertion_wave";
    public string Name => "Desertion Wave";

    public string Description =>
        """
        Morale has shattered. 
        In the dark hours before dawn, soldiers and workers slip through gaps in the walls... 
        choosing the unknown over a siege they no longer believe can be survived.
        """;

    const int MoraleThreshold = 30;
    const int Desertions = 10;

    public bool ShouldTrigger(GameState state)
    {
        return state.Morale < MoraleThreshold;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        entry.Write(
            "At dawn, the western gate stands open. Footprints lead into the fog. They chose the enemy over you.");
        state.ApplyWorkerDesertion(Desertions);
    }
}