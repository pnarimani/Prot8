using Prot8.Jobs;

namespace Prot8.Cli.Commands;

public sealed class AssignCommand : ICommand
{
    public required string JobId { get; init; }
    public int DeltaWorkers { get; init; }

    public CommandResult Execute(CommandContext context)
    {
        if (!TryResolveJob(JobId, out var job, out var jobReason))
        {
            return new CommandResult(false, jobReason);
        }

        var available = context.State.IdleWorkers;
        var currentForJob = context.State.Allocation.Workers[job];

        if (DeltaWorkers > available)
        {
            return new CommandResult(false,
                $"Not enough available workers to assign {DeltaWorkers} to {job}. Available: {available}, Current for job: {currentForJob}.");
        }

        if (currentForJob + DeltaWorkers < 0)
        {
            return new CommandResult(false,
                $"Cannot assign negative workers to {job}. Current: {currentForJob}, Attempted change: {DeltaWorkers}.");
        }


        var newAssignedWorkers = currentForJob + DeltaWorkers;
        context.State.Allocation.SetWorkers(job, newAssignedWorkers);
        return new CommandResult(true, $"Assigned {DeltaWorkers} workers to {job}. Remaining Idle: {available - DeltaWorkers}");
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