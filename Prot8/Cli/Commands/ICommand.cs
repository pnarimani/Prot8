namespace Prot8.Cli.Commands;

public interface ICommand
{
    CommandResult Execute(CommandContext context);
}
