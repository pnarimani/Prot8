using System.Text.Json.Serialization;
using Prot8.Buildings;
using Prot8.Constants;

namespace Prot8.Cli.Commands;

public sealed class ToggleBuildingCommand : ICommand
{
    [JsonPropertyName("building_id")]
    public required string BuildingId { get; init; }

    public CommandResult Execute(CommandContext context)
    {
        if (GameBalance.AllocationMode != WorkerAllocationMode.BuildingActivation)
            return new CommandResult(false,
                "Toggle command is only available in BuildingActivation allocation mode.");

        if (!Enum.TryParse<BuildingId>(BuildingId, true, out var id))
            return new CommandResult(false,
                $"Unknown building '{BuildingId}'. Valid: {string.Join(", ", Enum.GetNames<BuildingId>())}.");

        var building = context.State.GetBuilding(id);

        if (building.IsDestroyed)
            return new CommandResult(false, $"{building.Name} is destroyed and cannot be toggled.");

        building.IsActive = !building.IsActive;
        WorkerAllocationStrategy.ApplyAutomaticAllocation(context.State);

        var status = building.IsActive ? "Active" : "Inactive";
        return new CommandResult(true, $"{building.Name} is now {status}.");
    }
}
