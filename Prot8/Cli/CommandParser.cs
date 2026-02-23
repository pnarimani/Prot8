using Prot8.Cli.Commands;

namespace Prot8.Cli.Input;

public class CommandParser
{
    public bool TryParse(string rawInput, out ICommand? command, out string error)
    {
        command = null;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(rawInput))
        {
            error = "Command cannot be empty.";
            return false;
        }

        var parts = rawInput.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var name = parts[0].ToLowerInvariant();

        switch (name)
        {
            case "assign":
                if (parts.Length != 3)
                {
                    error = "Usage: assign <JobType> <Workers>.";
                    return false;
                }

                if (!int.TryParse(parts[2], out var workers))
                {
                    error = $"Invalid worker count '{parts[2]}'. Must be an integer.";
                    return false;
                }

                command = new AssignCommand
                {
                    JobId = parts[1],
                    DeltaWorkers = workers,
                };
                return true;

            case "clear_assignments":
                if (parts.Length != 1)
                {
                    error = "clear_assignments takes no parameters.";
                    return false;
                }

                command = new ClearAssignmentsCommand();
                return true;

            case "enact":
            case "enact_law":
                if (parts.Length != 2)
                {
                    error = "Usage: enact <LawId>.";
                    return false;
                }

                command = new EnactLawCommand
                {
                    LawId = parts[1],
                };
                return true;

            case "order":
            case "issue_order":
                if (parts.Length != 2)
                {
                    error = "Usage: order <OrderId>";
                    return false;
                }

                command = new IssueOrderCommand
                {
                    OrderId = parts[1],
                };
                return true;

            case "mission":
            case "start_mission":
                if (parts.Length != 2)
                {
                    error = "Usage: mission <MissionId>.";
                    return false;
                }

                command = new StartMissionCommand
                {
                    MissionId = parts[1],
                };
                return true;

            case "clear_action":
                if (parts.Length != 1)
                {
                    error = "clear_action takes no parameters.";
                    return false;
                }

                command = new ClearActionCommand();
                return true;

            case "end_day":
                if (parts.Length != 1)
                {
                    error = "end_day takes no parameters.";
                    return false;
                }

                command = new EndDayCommand();
                return true;

            default:
                error = $"Unknown command '{parts[0]}'.";
                return false;
        }
    }
}
