using System.Text.Json.Serialization;
using Prot8.Constants;
using Prot8.Resources;
using Prot8.Zones;

namespace Prot8.Cli.Commands;

public sealed class UpgradeStorageCommand : ICommand
{
    [JsonPropertyName("zone_id")]
    public required string ZoneIdStr { get; init; }

    public CommandResult Execute(CommandContext context)
    {
        if (!Enum.TryParse<ZoneId>(ZoneIdStr, true, out var zoneId))
        {
            return new CommandResult(false,
                $"Unknown zone '{ZoneIdStr}'. Valid zones: {string.Join(", ", Enum.GetNames<ZoneId>())}.");
        }

        var zone = context.State.GetZone(zoneId);
        if (zone.IsLost)
        {
            return new CommandResult(false, $"{zone.Name} is lost and cannot be upgraded.");
        }

        var storage = context.State.Resources.GetZoneStorage(zoneId);
        if (storage.UpgradeLevel >= GameBalance.StorageMaxUpgradeLevel)
        {
            return new CommandResult(false,
                $"{zone.Name} storage is already at max level ({GameBalance.StorageMaxUpgradeLevel}).");
        }

        if (!context.State.Resources.Has(ResourceKind.Materials, GameBalance.StorageUpgradeMaterialsCost))
        {
            return new CommandResult(false,
                $"Not enough materials. Need {GameBalance.StorageUpgradeMaterialsCost}, have {context.State.Resources[ResourceKind.Materials]}.");
        }

        context.State.Resources.Consume(ResourceKind.Materials, GameBalance.StorageUpgradeMaterialsCost);
        storage.UpgradeLevel++;

        return new CommandResult(true,
            $"Upgraded {zone.Name} storage to level {storage.UpgradeLevel}. Capacity per resource: {storage.Capacity}. Materials -{GameBalance.StorageUpgradeMaterialsCost}.");
    }
}
