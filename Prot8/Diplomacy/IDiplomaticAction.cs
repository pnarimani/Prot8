using Prot8.Simulation;

namespace Prot8.Diplomacy;

public interface IDiplomaticAction
{
    string Id { get; }
    string Name { get; }
    string GetTooltip(GameState state);
    bool CanActivate(GameState state);
    void OnActivate(GameState state, ResolutionEntry entry);
    void ApplyDaily(GameState state, ResolutionEntry entry);
    bool CanDeactivate { get; }
}
