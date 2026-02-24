using Prot8.Simulation;

namespace Prot8.Laws;

public interface ILaw
{
    string Id { get; }

    string Name { get; }

    string GetTooltip(GameState state);

    bool CanEnact(GameState state, out string reason);

    void OnEnact(GameState state, ResolutionEntry entry);

    void ApplyDaily(GameState state, ResolutionEntry entry);
}