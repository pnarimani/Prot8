using System.Text.Json.Serialization;

namespace Prot8.Cli.Commands;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AssignCommand), "assign")]
[JsonDerivedType(typeof(AddWorkers), "add_workers")]
[JsonDerivedType(typeof(RemoveWorkers), "remove_workers")]
[JsonDerivedType(typeof(ClearAssignmentsCommand), "clear_assignments")]
[JsonDerivedType(typeof(EnactLawCommand), "enact_law")]
[JsonDerivedType(typeof(IssueOrderCommand), "issue_order")]
[JsonDerivedType(typeof(StartMissionCommand), "start_mission")]
[JsonDerivedType(typeof(ClearActionCommand), "clear_action")]
[JsonDerivedType(typeof(EndDayCommand), "end_day")]
[JsonDerivedType(typeof(UpgradeStorageCommand), "upgrade_storage")]
[JsonDerivedType(typeof(PriorityCommand), "set_priority")]
[JsonDerivedType(typeof(ToggleBuildingCommand), "toggle_building")]
public interface ICommand
{
    CommandResult Execute(CommandContext context);
}
