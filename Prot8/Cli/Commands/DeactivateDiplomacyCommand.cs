using System.Text.Json.Serialization;
using Prot8.Constants;
using Prot8.Diplomacy;
using Prot8.Simulation;

namespace Prot8.Cli.Commands;

public sealed class DeactivateDiplomacyCommand : ICommand
{
    [JsonPropertyName("action_id")]
    public required string ActionId { get; init; }

    public CommandResult Execute(CommandContext context)
    {
        if (!GameBalance.EnableDiplomacy)
            return new CommandResult(false, "Diplomacy system is not enabled.");

        var action = DiplomacyCatalog.Find(ActionId);
        if (action is null)
            return new CommandResult(false, $"Unknown diplomatic action '{ActionId}'.");

        var state = context.State;

        if (!state.ActiveDiplomacyIds.Contains(action.Id))
            return new CommandResult(false, $"{action.Name} is not active.");

        if (!action.CanDeactivate)
            return new CommandResult(false, $"{action.Name} cannot be deactivated. This is a permanent commitment.");

        state.ActiveDiplomacyIds.Remove(action.Id);

        return new CommandResult(true, $"{action.Name} deactivated. Daily costs will stop.");
    }
}
