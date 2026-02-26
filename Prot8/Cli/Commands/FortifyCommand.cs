using System.Text.Json.Serialization;
using Prot8.Constants;
using Prot8.Resources;
using Prot8.Zones;

namespace Prot8.Cli.Commands;

public sealed class FortifyCommand : ICommand
{
    [JsonPropertyName("zone_id")]
    public required string ZoneIdStr { get; init; }

    public CommandResult Execute(CommandContext context)
    {
        if (!GameBalance.EnableFortifications)
        {
            return new CommandResult(false, "Fortifications are not enabled.");
        }

        if (!Enum.TryParse<ZoneId>(ZoneIdStr, true, out var zoneId))
        {
            return new CommandResult(false,
                $"Unknown zone '{ZoneIdStr}'. Valid zones: {string.Join(", ", Enum.GetNames<ZoneId>())}.");
        }

        var zone = context.State.GetZone(zoneId);

        if (zone.IsLost)
        {
            return new CommandResult(false, $"{zone.Name} is lost and cannot be fortified.");
        }

        if (zone.FortificationLevel >= GameBalance.FortificationMaxLevel)
        {
            return new CommandResult(false,
                $"{zone.Name} is already at max fortification level ({GameBalance.FortificationMaxLevel}).");
        }

        if (!context.State.Resources.Has(ResourceKind.Materials, GameBalance.FortificationMaterialsCost))
        {
            return new CommandResult(false,
                $"Not enough materials. Need {GameBalance.FortificationMaterialsCost}, have {context.State.Resources[ResourceKind.Materials]}.");
        }

        context.State.Resources.Consume(ResourceKind.Materials, GameBalance.FortificationMaterialsCost);
        zone.FortificationLevel++;

        return new CommandResult(true,
            $"Fortified {zone.Name} to level {zone.FortificationLevel}. Siege damage reduction: -{zone.FortificationLevel * GameBalance.FortificationDamageReductionPerLevel}. Materials -{GameBalance.FortificationMaterialsCost}.");
    }
}
