using Prot8.Jobs;

namespace Prot8.Cli.Input.Commands;

public sealed class ClearAssignmentsCommand : ICommand
{
    public CommandResult Execute(CommandContext context)
    {
        foreach (var job in Enum.GetValues<JobType>())
            context.State.Allocation.SetWorkers(job, 0);

        context.State.Allocation.SetIdleWorkers(context.State.AvailableHealthyWorkersForAllocation);
        return new CommandResult(true, "All job assignments cleared.");
    }
}
