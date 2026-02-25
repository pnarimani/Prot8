using System.Text.Json.Serialization;
using Prot8.Buildings;

namespace Prot8.Cli.Commands;

public class AddWorkers : AssignCommand
{
}

public class RemoveWorkers : AssignCommand
{
    public override CommandResult Execute(CommandContext context)
    {
        return Perform(context, -Math.Abs(DeltaWorkers), BuildingIdStr);
    }
}

public class AssignCommand : ICommand
{
    [JsonPropertyName("building_id")]
    public required string BuildingIdStr { get; init; }
    public int DeltaWorkers { get; init; }

    public virtual CommandResult Execute(CommandContext context)
    {
        return Perform(context, DeltaWorkers, BuildingIdStr);
    }

    protected static CommandResult Perform(CommandContext context, int deltaWorkers, string buildingIdStr)
    {
        if (!TryResolveBuilding(buildingIdStr, out var buildingId, out var reason))
        {
            return new CommandResult(false, reason);
        }

        var building = context.State.GetBuilding(buildingId);

        if (building.IsDestroyed)
        {
            return new CommandResult(false, $"{building.Name} is destroyed and cannot have workers assigned.");
        }

        var available = context.State.IdleWorkers;
        var current = building.AssignedWorkers;

        if (deltaWorkers > available)
        {
            return new CommandResult(false,
                $"Not enough available workers to assign {deltaWorkers} to {building.Name}. Available: {available}, Current: {current}/{building.MaxWorkers}.");
        }

        if (current + deltaWorkers < 0)
        {
            return new CommandResult(false,
                $"Cannot assign negative workers to {building.Name}. Current: {current}, Attempted change: {deltaWorkers}.");
        }

        var newCount = current + deltaWorkers;
        if (newCount > building.MaxWorkers)
        {
            return new CommandResult(false,
                $"Cannot assign {newCount} workers to {building.Name} (max {building.MaxWorkers}). Current: {current}.");
        }

        context.State.Allocation.SetWorkers(buildingId, newCount);
        return new CommandResult(true,
            $"Assigned {deltaWorkers} workers to {building.Name}. Workers: {newCount}/{building.MaxWorkers}. Remaining Idle: {available - deltaWorkers}");
    }

    static bool TryResolveBuilding(string token, out BuildingId buildingId, out string reason)
    {
        if (Enum.TryParse(token, true, out buildingId))
        {
            reason = string.Empty;
            return true;
        }

        reason = $"Unknown building '{token}'. Valid buildings: {string.Join(", ", Enum.GetNames<BuildingId>())}.";
        return false;
    }
}
