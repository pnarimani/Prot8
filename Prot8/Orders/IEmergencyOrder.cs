using Prot8.Simulation;

namespace Prot8.Orders;

public interface IEmergencyOrder
{
    string Id { get; }

    string Name { get; }

    int CooldownDays { get; }

    string GetTooltip(GameState state);

    bool CanIssue(GameState state);

    void Apply(GameState state, ResolutionEntry entry);
}