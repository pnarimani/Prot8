using Prot8.Simulation;

namespace Prot8.Cli.Commands;

public sealed class CommandContext(GameState state, TurnActionChoice action)
{
    public GameState State { get; } = state;
    public TurnActionChoice Action { get; set; } = action;
}
