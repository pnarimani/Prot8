namespace Prot8.Cli.Input.Commands;

public interface ICommand
{
    CommandResult Execute(CommandContext context);
}
