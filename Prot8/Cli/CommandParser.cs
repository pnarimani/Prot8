using Prot8.Cli.Commands;
using Prot8.Constants;

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
                if (GameBalance.AllocationMode != WorkerAllocationMode.ManualAssignment)
                {
                    error = "Manual assignment is disabled in the current allocation mode.";
                    return false;
                }

                if (parts.Length != 3)
                {
                    error = "Usage: assign <Building> <Workers>.";
                    return false;
                }

                if (parts[2] == "max")
                {
                    command = new AssignCommand
                    {
                        BuildingId = parts[1],
                        DeltaWorkers = int.MaxValue, 
                    };
                    return true;
                }
                
                if(parts[2] == "none")
                {
                    command = new AssignCommand
                    {
                        BuildingId = parts[1],
                        DeltaWorkers = int.MinValue, 
                    };
                    return true;
                }

                if (!int.TryParse(parts[2], out var workers))
                {
                    error = $"Invalid worker count '{parts[2]}'. Must be an integer.";
                    return false;
                }

                command = new AssignCommand
                {
                    BuildingId = parts[1],
                    DeltaWorkers = workers,
                };
                return true;

            case "clear_assignments":
                if (GameBalance.AllocationMode != WorkerAllocationMode.ManualAssignment)
                {
                    error = "Manual assignment is disabled in the current allocation mode.";
                    return false;
                }

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

            case "upgrade":
                if (parts.Length != 2)
                {
                    error = "Usage: upgrade <Zone>.";
                    return false;
                }

                command = new UpgradeStorageCommand { ZoneIdStr = parts[1] };
                return true;

            case "priority":
                if (GameBalance.AllocationMode != WorkerAllocationMode.PriorityQueue)
                {
                    error = "Priority command is only available in PriorityQueue allocation mode.";
                    return false;
                }

                if (parts.Length < 2)
                {
                    error = "Usage: priority <Resource1> <Resource2> ...";
                    return false;
                }

                command = new PriorityCommand
                {
                    Priorities = parts[1..].ToList(),
                };
                return true;

            case "set_recipe":
                if (parts.Length != 2)
                {
                    error = "Usage: set_recipe <normal|gruel|feast>.";
                    return false;
                }

                command = new SetRecipeCommand
                {
                    RecipeStr = parts[1],
                };
                return true;

            case "build_defense":
                if (parts.Length != 3)
                {
                    error = "Usage: build_defense <type> <zone_id>.";
                    return false;
                }

                command = new BuildDefenseCommand
                {
                    DefenseTypeStr = parts[1],
                    ZoneIdStr = parts[2],
                };
                return true;

            case "assign_archers":
                if (parts.Length != 3)
                {
                    error = "Usage: assign_archers <zone_id> <count>.";
                    return false;
                }

                if (!int.TryParse(parts[2], out var archerCount))
                {
                    error = $"Invalid count '{parts[2]}'. Must be an integer.";
                    return false;
                }

                command = new AssignArcherPostCommand
                {
                    ZoneIdStr = parts[1],
                    Count = archerCount,
                };
                return true;

            case "specialize_clinic":
                if (parts.Length != 2)
                {
                    error = "Usage: specialize_clinic <hospital|quarantine_ward>.";
                    return false;
                }

                command = new SpecializeClinicCommand
                {
                    SpecializationStr = parts[1],
                };
                return true;

            case "fortify":
                if (parts.Length != 2)
                {
                    error = "Usage: fortify <Zone>.";
                    return false;
                }

                command = new FortifyCommand
                {
                    ZoneIdStr = parts[1],
                };
                return true;

            case "upgrade_building":
                if (parts.Length != 2)
                {
                    error = "Usage: upgrade_building <Building>.";
                    return false;
                }

                command = new UpgradeBuildingCommand
                {
                    BuildingIdStr = parts[1],
                };
                return true;

            case "toggle":
                if (GameBalance.AllocationMode != WorkerAllocationMode.BuildingActivation)
                {
                    error = "Toggle command is only available in BuildingActivation allocation mode.";
                    return false;
                }

                if (parts.Length != 2)
                {
                    error = "Usage: toggle <Building>";
                    return false;
                }

                command = new ToggleBuildingCommand
                {
                    BuildingId = parts[1],
                };
                return true;

            default:
                error = $"Unknown command '{parts[0]}'.";
                return false;
        }
    }
}
