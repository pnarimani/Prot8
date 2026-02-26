using System.Text.Json.Serialization;
using Prot8.Buildings;
using Prot8.Constants;
using Prot8.Resources;

namespace Prot8.Cli.Commands;

public sealed class UpgradeBuildingCommand : ICommand
{
    [JsonPropertyName("building_id")]
    public required string BuildingIdStr { get; init; }

    public CommandResult Execute(CommandContext context)
    {
        if (!GameBalance.EnableBuildingUpgrades)
        {
            return new CommandResult(false, "Building upgrades are not enabled.");
        }

        if (!Enum.TryParse<BuildingId>(BuildingIdStr, true, out var buildingId))
        {
            return new CommandResult(false,
                $"Unknown building '{BuildingIdStr}'. Valid buildings: {string.Join(", ", Enum.GetNames<BuildingId>())}.");
        }

        var building = context.State.GetBuilding(buildingId);

        if (building.IsDestroyed)
        {
            return new CommandResult(false, $"{building.Name} is destroyed and cannot be upgraded.");
        }

        if (building.UpgradeLevel >= GameBalance.BuildingMaxUpgradeLevel)
        {
            return new CommandResult(false,
                $"{building.Name} is already at max upgrade level ({GameBalance.BuildingMaxUpgradeLevel}).");
        }

        if (building.UpgradeDaysRemaining > 0)
        {
            return new CommandResult(false,
                $"{building.Name} is already being upgraded ({building.UpgradeDaysRemaining} day(s) remaining).");
        }

        if (!context.State.Resources.Has(ResourceKind.Materials, GameBalance.BuildingUpgradeMaterialsCost))
        {
            return new CommandResult(false,
                $"Not enough materials. Need {GameBalance.BuildingUpgradeMaterialsCost}, have {context.State.Resources[ResourceKind.Materials]}.");
        }

        context.State.Resources.Consume(ResourceKind.Materials, GameBalance.BuildingUpgradeMaterialsCost);

        var delay = GameBalance.BuildingUpgradeDelayDays;
        if (delay <= 0)
        {
            building.UpgradeLevel++;
            return new CommandResult(true,
                $"Upgraded {building.Name} to level {building.UpgradeLevel}. Production bonus: +{building.UpgradeLevel * GameBalance.BuildingUpgradeBonusPerLevel * 100:F0}%. Materials -{GameBalance.BuildingUpgradeMaterialsCost}.");
        }

        building.UpgradeDaysRemaining = delay;
        return new CommandResult(true,
            $"Upgrade started for {building.Name}. Completes in {delay} day(s). Materials -{GameBalance.BuildingUpgradeMaterialsCost}.");
    }
}
