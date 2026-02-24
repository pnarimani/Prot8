using Prot8.Simulation;

namespace Prot8.Events;

public sealed class DespairEvent() : ITriggeredEvent
{
    public string Id => "despair";
    public string Name => "Wave of Despair";
    public string Description => "The weight of the siege has broken something in the city. People move like ghosts. The will to endure is fraying at its edges.";

    const int MinimumDay = 10;
    const int MoraleThreshold = 45;
    const int TriggerChance = 15;
    const int MoraleLoss = 10;
    const int UnrestGain = 8;
    const int Desertions = 3;

    public bool ShouldTrigger(GameState state)
    {
        if (state.Day < MinimumDay)
        {
            return false;
        }

        if (state.Morale >= MoraleThreshold)
        {
            return false;
        }

        return state.Random.Next(1, 101) <= TriggerChance;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        entry.Write("A pall of hopelessness settles over the city. Workers sit idle. Soldiers stare at the walls. The siege has worn through to the soul.");
        state.AddMorale(-MoraleLoss, entry);
        state.AddUnrest(UnrestGain, entry);
    }
}