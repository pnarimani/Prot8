using Prot8.Jobs;

namespace Prot8.Cli.Commands;

public sealed class AssignCommand : ICommand
{
    public required string JobId { get; init; }
    public int Workers { get; init; }

    public CommandResult Execute(CommandContext context)
    {
        if (!TryResolveJob(JobId, out var job, out var jobReason))
        {
            return new CommandResult(false, jobReason);
        }

        if (Workers < 0)
        {
            return new CommandResult(false, "Workers cannot be negative.");
        }

        var available = context.State.AvailableHealthyWorkersForAllocation;
        var currentForJob = context.State.Allocation.Workers[job];
        var newTotalAssigned = context.State.Allocation.TotalAssigned() - currentForJob + Workers;
        if (newTotalAssigned > available)
        {
            return new CommandResult(false, $"Assignment exceeds available workers ({newTotalAssigned}/{available}).");
        }

        context.State.Allocation.SetWorkers(job, Workers);
        context.State.Allocation.SetIdleWorkers(available - newTotalAssigned);
        return new CommandResult(true,
            $"Assigned {Workers} workers to {job}. Total assigned: {newTotalAssigned}/{available}. Idle: {available - newTotalAssigned}.");
    }

    static bool TryResolveJob(string token, out JobType job, out string reason)
    {
        if (Enum.TryParse(token, true, out job))
        {
            reason = string.Empty;
            return true;
        }

        reason = $"Unknown JobType '{token}'. Valid types: {string.Join(", ", Enum.GetNames<JobType>())}.";
        return false;
    }
}