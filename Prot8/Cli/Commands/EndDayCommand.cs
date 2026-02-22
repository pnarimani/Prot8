namespace Prot8.Cli.Commands;

public sealed class EndDayCommand : ICommand
{
    public CommandResult Execute(CommandContext context)
    {
        return new CommandResult(true, "Day resolution requested.", EndDayRequested: true);
    }
}
