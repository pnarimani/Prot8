using Prot8.Jobs;

namespace Prot8.Cli.Input.Commands;

public sealed class AssignCommand(string jobToken, string workersToken) : ICommand
{
    public CommandResult Execute(CommandContext context)
    {
        if (!TryResolveJob(jobToken, out var job, out var jobReason))
            return new CommandResult(false, jobReason);

        if (!int.TryParse(workersToken, out var workers))
            return new CommandResult(false, "Workers must be a whole number.");

        if (workers < 0)
            return new CommandResult(false, "Workers cannot be negative.");

        if (workers % JobAllocation.Step != 0)
            return new CommandResult(false, $"Workers must be in increments of {JobAllocation.Step}.");

        var available = context.State.AvailableHealthyWorkersForAllocation;
        var currentForJob = context.State.Allocation.Workers[job];
        var newTotalAssigned = context.State.Allocation.TotalAssigned() - currentForJob + workers;
        if (newTotalAssigned > available)
            return new CommandResult(false, $"Assignment exceeds available workers ({newTotalAssigned}/{available}).");

        context.State.Allocation.SetWorkers(job, workers);
        context.State.Allocation.SetIdleWorkers(available - newTotalAssigned);
        return new CommandResult(true,
            $"Assigned {workers} workers to {job}. Total assigned: {newTotalAssigned}/{available}. Idle: {available - newTotalAssigned}.");
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
