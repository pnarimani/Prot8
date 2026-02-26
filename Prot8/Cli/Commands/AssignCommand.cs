using Prot8.Buildings;

namespace Prot8.Cli.Commands;

public class AddWorkers : AssignCommand
{
}

public class RemoveWorkers : AssignCommand
{
    public override CommandResult Execute(CommandContext context)
    {
        return Perform(context, -Math.Abs(DeltaWorkers), BuildingId);
    }
}

public class AssignCommand : ICommand
{
    public required string BuildingId { get; init; }
    public int DeltaWorkers { get; init; }

    public virtual CommandResult Execute(CommandContext context) => Perform(context, DeltaWorkers, BuildingId);

    protected static CommandResult Perform(CommandContext context, int deltaWorkers, string buildingIdStr)
    {
        if (!TryResolveBuilding(buildingIdStr, out var buildingId, out var reason))
            return new CommandResult(false, reason);

        var building = context.State.GetBuilding(buildingId);

        if (building.IsDestroyed)
            return new CommandResult(false, $"{building.Name} is destroyed and cannot have workers assigned.");

        var available = context.State.IdleWorkers;
        var current = building.AssignedWorkers;

        if (deltaWorkers > 0)
        {
            var canAssign = building.MaxWorkers - current;
            deltaWorkers = Math.Min(deltaWorkers, canAssign);
        }

        if (deltaWorkers < 0)
            deltaWorkers = Math.Max(deltaWorkers, -current);

        var newCount = current + deltaWorkers;

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