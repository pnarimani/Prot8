using System.Text.Json.Serialization;
using Prot8.Buildings;
using Prot8.Constants;
using Prot8.Resources;

namespace Prot8.Cli.Commands;

public sealed class PriorityCommand : ICommand
{
    [JsonPropertyName("priorities")]
    public required List<string> Priorities { get; init; }

    public CommandResult Execute(CommandContext context)
    {
        if (GameBalance.AllocationMode != WorkerAllocationMode.PriorityQueue)
            return new CommandResult(false, "Priority command is only available in PriorityQueue allocation mode.");

        var parsed = new List<ResourceKind>();
        var seen = new HashSet<ResourceKind>();

        foreach (var s in Priorities)
        {
            if (!Enum.TryParse<ResourceKind>(s, true, out var kind))
                return new CommandResult(false,
                    $"Unknown resource '{s}'. Valid: {string.Join(", ", Enum.GetNames<ResourceKind>())}.");

            if (!seen.Add(kind))
                return new CommandResult(false, $"Duplicate resource '{s}'.");

            parsed.Add(kind);
        }

        // Append any missing kinds at the end
        foreach (var kind in Enum.GetValues<ResourceKind>())
        {
            if (seen.Add(kind))
                parsed.Add(kind);
        }

        context.State.ResourcePriority = parsed;
        WorkerAllocationStrategy.ApplyAutomaticAllocation(context.State);

        var order = string.Join(" > ", parsed.Select(k => k.ToString()));
        return new CommandResult(true, $"Resource priority set: {order}");
    }
}
