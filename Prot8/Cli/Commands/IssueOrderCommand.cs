using Prot8.Simulation;
using Prot8.Zones;

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

        if (!order.CanIssue(context.State, out var reason))
        {
            return new CommandResult(false, $"Cannot issue {order.Name}: {reason}");
        }

        context.Action = new TurnActionChoice
        {
            EmergencyOrderId = order.Id,
        };

        return new CommandResult(true, $"Queued emergency order for today: {order.Name}.");
    }

    static bool TryParseZone(string token, out ZoneId zone)
    {
        if (Enum.TryParse(token, true, out zone))
        {
            return true;
        }

        if (int.TryParse(token, out var raw) && raw >= (int)ZoneId.OuterFarms && raw <= (int)ZoneId.Keep)
        {
            zone = (ZoneId)raw;
            return true;
        }

        return false;
    }
}