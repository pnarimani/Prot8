using Prot8.Jobs;
using Prot8.Simulation;

namespace Prot8.Cli.Input.Commands;

public sealed class CommandContext(GameState state, TurnActionChoice action)
{
    public GameState State { get; } = state;
    public TurnActionChoice Action { get; set; } = action;
}
