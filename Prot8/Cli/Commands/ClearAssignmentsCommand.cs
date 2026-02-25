namespace Prot8.Cli.Commands;

public sealed class ClearAssignmentsCommand : ICommand
{
    public CommandResult Execute(CommandContext context)
    {
        context.State.Allocation.Clear();
        return new CommandResult(true, "All building assignments cleared.");
    }
}
