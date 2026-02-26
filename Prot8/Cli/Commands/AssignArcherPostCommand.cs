using System.Text.Json.Serialization;
using Prot8.Constants;
using Prot8.Zones;

namespace Prot8.Cli.Commands;

public sealed class AssignArcherPostCommand : ICommand
{
    [JsonPropertyName("zone_id")]
    public required string ZoneIdStr { get; init; }

    [JsonPropertyName("count")]
    public required int Count { get; init; }

    public CommandResult Execute(CommandContext context)
    {
        if (!GameBalance.EnableDefenses)
        {
            return new CommandResult(false, "Defenses are not enabled.");
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
        if (!defenses.HasArcherPost)
        {
            return new CommandResult(false, $"{zone.Name} does not have an archer post. Build one first.");
        }

        if (Count < 0)
        {
            return new CommandResult(false, "Count must be non-negative.");
        }

        if (Count > GameBalance.ArcherPostGuardsRequired)
        {
            return new CommandResult(false,
                $"Archer post only requires {GameBalance.ArcherPostGuardsRequired} guards.");
        }

        defenses.ArcherPostGuardsAssigned = Count;
        return new CommandResult(true,
            $"{Count} guards assigned to archer post in {zone.Name}.");
    }
}
