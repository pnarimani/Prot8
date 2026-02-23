using Prot8.Simulation;

namespace Prot8.Decrees;

public interface IDecree
{
    string Id { get; }

    string Name { get; }

    string GetTooltip(GameState state);

    bool CanIssue(GameState state, out string reason);

    void Apply(GameState state, DayResolutionReport report);
}
