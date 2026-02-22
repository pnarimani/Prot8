using System.Text.Json.Serialization;
using Prot8.Simulation;

namespace Prot8.Cli.Commands;

public sealed class EnactLawCommand : ICommand
{
    public required string LawId { get; init; }
    
    public CommandResult Execute(CommandContext context)
    {
        var available = ActionAvailability.GetAvailableLaws(context.State);
        var law = available.FirstOrDefault(a => a.Id.Equals(LawId, StringComparison.OrdinalIgnoreCase));
        if (law == null)
            return new CommandResult(false, $"Failed to find law with Id {LawId}");

        context.Action = new TurnActionChoice { LawId = law.Id };
        return new CommandResult(true, $"Queued law for today: {law.Name}.");
    }
}
