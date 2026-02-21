using Prot8.Jobs;
using Prot8.Simulation;

namespace Prot8.Cli.Input;

public sealed class DayCommandPlan
{
    public DayCommandPlan(JobAllocation allocation, TurnActionChoice action)
    {
        Allocation = allocation;
        Action = action;
    }

    public JobAllocation Allocation { get; }

    public TurnActionChoice Action { get; }
}