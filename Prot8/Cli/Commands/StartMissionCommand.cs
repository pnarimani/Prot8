using Prot8.Simulation;

namespace Prot8.Cli.Commands;

public sealed class StartMissionCommand : ICommand
{
    public required string MissionId { get; init; }

    public CommandResult Execute(CommandContext context)
    {
        var available = ActionAvailability.GetAvailableMissions(context.State);
        var mission = available.FirstOrDefault(m => m.Id.Equals(MissionId, StringComparison.OrdinalIgnoreCase));
        if (mission == null)
        {
            return new CommandResult(false, $"Failed to find mission with id {MissionId}");
        }

        context.Action = new TurnActionChoice { MissionId = mission!.Id };
        return new CommandResult(true, $"Queued mission for today: {mission.Name}.");
    }
}