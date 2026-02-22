using Prot8.Simulation;

namespace Prot8.Cli.Commands;

public sealed class ClearActionCommand : ICommand
{
    public CommandResult Execute(CommandContext context)
    {
        context.Action = new TurnActionChoice();
        return new CommandResult(true, "Queued day action cleared.");
    }
}
