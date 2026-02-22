using Prot8.Simulation;

namespace Prot8.Cli.Input.Commands;

public sealed class EnactLawCommand(string lawToken) : ICommand
{
    public CommandResult Execute(CommandContext context)
    {
        var available = ActionAvailability.GetAvailableLaws(context.State);
        var law = available.FirstOrDefault(a => a.Id.Equals(lawToken, StringComparison.OrdinalIgnoreCase));
        if (law == null)
            return new CommandResult(false, $"Failed to find law with Id {lawToken}");

        context.Action = new TurnActionChoice { LawId = law.Id };
        return new CommandResult(true, $"Queued law for today: {law.Name}.");
    }
}
