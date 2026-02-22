using Prot8.Jobs;

namespace Prot8.Cli.Commands;

public class AddWorkers : AssignCommand
{
}

public class RemoveWorkers : AssignCommand
{
    public override CommandResult Execute(CommandContext context)
    {
        return Perform(context, -Math.Abs(DeltaWorkers), JobId);
    }
}

public class AssignCommand : ICommand
{
    public required string JobId { get; init; }
    public int DeltaWorkers { get; init; }

    public virtual CommandResult Execute(CommandContext context)
    {
        return Perform(context, DeltaWorkers, JobId);
    }

    protected static CommandResult Perform(CommandContext context, int deltaWorkers, string jobId)
    {
        if (!TryResolveJob(jobId, out var job, out var jobReason))
        {
            return new CommandResult(false, jobReason);
        }

        var available = context.State.IdleWorkers;
        var currentForJob = context.State.Allocation.Workers[job];

        if (deltaWorkers > available)
        {
            return new CommandResult(false,
                $"Not enough available workers to assign {deltaWorkers} to {job}. Available: {available}, Current for job: {currentForJob}.");
        }

        if (currentForJob + deltaWorkers < 0)
        {
            return new CommandResult(false,
                $"Cannot assign negative workers to {job}. Current: {currentForJob}, Attempted change: {deltaWorkers}.");
        }


        var newAssignedWorkers = currentForJob + deltaWorkers;
        context.State.Allocation.SetWorkers(job, newAssignedWorkers);
        return new CommandResult(true,
            $"Assigned {deltaWorkers} workers to {job}. Remaining Idle: {available - deltaWorkers}");
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