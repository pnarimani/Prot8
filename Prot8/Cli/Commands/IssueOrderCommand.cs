using Prot8.Simulation;

namespace Prot8.Cli.Commands;

public sealed class IssueOrderCommand : ICommand
{
    public required string OrderId { get; init; }

    public CommandResult Execute(CommandContext context)
    {
        var available = ActionAvailability.GetAvailableOrders(context.State);
        var order = available.FirstOrDefault(o => o.Id.Equals(OrderId, StringComparison.OrdinalIgnoreCase));
        if (order == null)
        {
            return new CommandResult(false, $"Failed to find order with id {OrderId}");
        }

        context.Action = new TurnActionChoice
        {
            EmergencyOrderId = order.Id,
        };

        return new CommandResult(true, $"Queued emergency order for today: {order.Name}.");
    }
}