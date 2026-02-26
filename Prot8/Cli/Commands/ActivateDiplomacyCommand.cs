using System.Text.Json.Serialization;
using Prot8.Constants;
using Prot8.Diplomacy;
using Prot8.Simulation;

namespace Prot8.Cli.Commands;

public sealed class ActivateDiplomacyCommand : ICommand
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

        if (state.ActiveDiplomacyIds.Contains(action.Id))
            return new CommandResult(false, $"{action.Name} is already active.");

        if (!action.CanActivate(state))
            return new CommandResult(false, $"Cannot activate {action.Name}. Requirements not met.");

        state.ActiveDiplomacyIds.Add(action.Id);
        var entry = new ResolutionEntry { Title = $"Diplomacy: {action.Name}" };
        action.OnActivate(state, entry);

        return new CommandResult(true, $"{action.Name} activated. {action.GetTooltip(state)}");
    }
}
