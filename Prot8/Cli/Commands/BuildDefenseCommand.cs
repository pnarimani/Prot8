using System.Text.Json.Serialization;
using Prot8.Constants;
using Prot8.Defenses;
using Prot8.Resources;
using Prot8.Zones;

namespace Prot8.Cli.Commands;

public sealed class BuildDefenseCommand : ICommand
{
    [JsonPropertyName("defense_type")]
    public required string DefenseTypeStr { get; init; }

    [JsonPropertyName("zone_id")]
    public required string ZoneIdStr { get; init; }

    public CommandResult Execute(CommandContext context)
    {
        if (!GameBalance.EnableDefenses)
        {
            return new CommandResult(false, "Defenses are not enabled.");
        }

        if (!Enum.TryParse<DefenseType>(DefenseTypeStr, true, out var defenseType))
        {
            return new CommandResult(false,
                $"Unknown defense type '{DefenseTypeStr}'. Valid types: {string.Join(", ", Enum.GetNames<DefenseType>())}.");
        }

        if (!Enum.TryParse<ZoneId>(ZoneIdStr, true, out var zoneId))
        {
            return new CommandResult(false,
                $"Unknown zone '{ZoneIdStr}'. Valid zones: {string.Join(", ", Enum.GetNames<ZoneId>())}.");
        }

        var zone = context.State.GetZone(zoneId);
        if (zone.IsLost)
        {
            return new CommandResult(false, $"{zone.Name} is lost.");
        }

        var defenses = context.State.GetZoneDefenses(zoneId);

        return defenseType switch
        {
            DefenseType.Barricades => BuildBarricades(context, zone, defenses),
            DefenseType.OilCauldrons => BuildOilCauldron(context, zone, defenses),
            DefenseType.ArcherPosts => BuildArcherPost(context, zone, defenses),
            _ => new CommandResult(false, "Unknown defense type."),
        };
    }

    static CommandResult BuildBarricades(CommandContext context, ZoneState zone, ZoneDefenses defenses)
    {
        if (!context.State.Resources.Has(ResourceKind.Materials, GameBalance.BarricadeMaterialsCost))
        {
            return new CommandResult(false,
                $"Not enough materials. Need {GameBalance.BarricadeMaterialsCost}, have {context.State.Resources[ResourceKind.Materials]}.");
        }

        context.State.Resources.Consume(ResourceKind.Materials, GameBalance.BarricadeMaterialsCost);
        defenses.BarricadeBuffer += GameBalance.BarricadeBufferAmount;
        return new CommandResult(true,
            $"Barricades built in {zone.Name}. Buffer: {defenses.BarricadeBuffer}. Materials -{GameBalance.BarricadeMaterialsCost}.");
    }

    static CommandResult BuildOilCauldron(CommandContext context, ZoneState zone, ZoneDefenses defenses)
    {
        if (defenses.HasOilCauldron)
        {
            return new CommandResult(false, $"{zone.Name} already has an oil cauldron prepared.");
        }

        if (!context.State.Resources.Has(ResourceKind.Fuel, GameBalance.OilCauldronFuelCost))
        {
            return new CommandResult(false,
                $"Not enough fuel. Need {GameBalance.OilCauldronFuelCost}, have {context.State.Resources[ResourceKind.Fuel]}.");
        }

        if (!context.State.Resources.Has(ResourceKind.Materials, GameBalance.OilCauldronMaterialsCost))
        {
            return new CommandResult(false,
                $"Not enough materials. Need {GameBalance.OilCauldronMaterialsCost}, have {context.State.Resources[ResourceKind.Materials]}.");
        }

        context.State.Resources.Consume(ResourceKind.Fuel, GameBalance.OilCauldronFuelCost);
        context.State.Resources.Consume(ResourceKind.Materials, GameBalance.OilCauldronMaterialsCost);
        defenses.HasOilCauldron = true;
        return new CommandResult(true,
            $"Oil cauldron prepared in {zone.Name}. Will negate 1 day of siege damage. Fuel -{GameBalance.OilCauldronFuelCost}, Materials -{GameBalance.OilCauldronMaterialsCost}.");
    }

    static CommandResult BuildArcherPost(CommandContext context, ZoneState zone, ZoneDefenses defenses)
    {
        if (defenses.HasArcherPost)
        {
            return new CommandResult(false, $"{zone.Name} already has an archer post.");
        }

        if (!context.State.Resources.Has(ResourceKind.Materials, GameBalance.ArcherPostMaterialsCost))
        {
            return new CommandResult(false,
                $"Not enough materials. Need {GameBalance.ArcherPostMaterialsCost}, have {context.State.Resources[ResourceKind.Materials]}.");
        }

        context.State.Resources.Consume(ResourceKind.Materials, GameBalance.ArcherPostMaterialsCost);
        defenses.HasArcherPost = true;
        return new CommandResult(true,
            $"Archer post built in {zone.Name}. Assign {GameBalance.ArcherPostGuardsRequired} guards for -{GameBalance.ArcherPostDamageReduction * 100:F0}% siege damage. Materials -{GameBalance.ArcherPostMaterialsCost}.");
    }
}
