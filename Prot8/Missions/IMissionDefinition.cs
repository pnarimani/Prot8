using Prot8.Simulation;

namespace Prot8.Missions;

public interface IMissionDefinition
{
    string Id { get; }

    string Name { get; }

    int DurationDays { get; }

    int WorkerCost { get; }

    int GuardCost => 0;
    
    string GetTooltip(GameState state);

    bool CanStart(GameState state, out string reason);

    void ResolveOutcome(GameState state, ActiveMission mission, ResolutionEntry entry);
}