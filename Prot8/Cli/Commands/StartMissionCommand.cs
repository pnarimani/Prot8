using Prot8.Simulation;

namespace Prot8.Cli.Input.Commands;

public sealed class StartMissionCommand(string missionToken) : ICommand
{
    public CommandResult Execute(CommandContext context)
    {
        var available = ActionAvailability.GetAvailableMissions(context.State);
        var mission = available.FirstOrDefault(m => m.Id.Equals(missionToken, StringComparison.OrdinalIgnoreCase));
        if (mission == null)
            return new CommandResult(false, $"Failed to find mission with id {missionToken}");

        context.Action = new TurnActionChoice { MissionId = mission!.Id };
        return new CommandResult(true, $"Queued mission for today: {mission.Name}.");
    }
}