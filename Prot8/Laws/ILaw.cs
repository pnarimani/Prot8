using Prot8.Simulation;

namespace Prot8.Laws;

public interface ILaw
{
    string Id { get; }

    string Name { get; }

    string Summary { get; }

    bool CanEnact(GameState state, out string reason);

    void OnEnact(GameState state, DayResolutionReport report);

    void ApplyDaily(GameState state, DayResolutionReport report);
}