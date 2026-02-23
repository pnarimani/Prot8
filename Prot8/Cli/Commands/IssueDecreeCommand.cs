using Prot8.Decrees;
using Prot8.Simulation;

namespace Prot8.Cli.Commands;

public sealed class IssueDecreeCommand : ICommand
{
    public required string DecreeId { get; init; }

    public CommandResult Execute(CommandContext context)
    {
        var available = GetAvailableDecrees(context.State);
        var decree = available.FirstOrDefault(d => d.Id.Equals(DecreeId, StringComparison.OrdinalIgnoreCase));
        if (decree == null)
        {
            return new CommandResult(false, $"Failed to find decree with id {DecreeId}");
        }

        context.Action = new TurnActionChoice
        {
            LawId = context.Action.LawId,
            EmergencyOrderId = context.Action.EmergencyOrderId,
            MissionId = context.Action.MissionId,
            DecreeId = decree.Id,
        };

        return new CommandResult(true, $"Queued decree for today: {decree.Name}.");
    }

    static IReadOnlyList<IDecree> GetAvailableDecrees(GameState state)
    {
        var available = new List<IDecree>();
        foreach (var decree in DecreeCatalog.GetAll())
        {
            if (decree.CanIssue(state, out _))
                available.Add(decree);
        }

        return available;
    }
}
