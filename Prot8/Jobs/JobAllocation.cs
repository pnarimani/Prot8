using System;
using System.Collections.Generic;
using System.Linq;

namespace Prot8.Jobs;

public sealed class JobAllocation
{
    public const int Step = 5;

    private readonly Dictionary<JobType, int> _workers = Enum.GetValues<JobType>().ToDictionary(job => job, _ => 0);

    public int IdleWorkers { get; private set; }

    public IReadOnlyDictionary<JobType, int> Workers => _workers;

    public void SetWorkers(JobType job, int workers)
    {
        if (workers < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(workers));
        }

        if (workers % Step != 0)
        {
            throw new ArgumentException($"Workers for {job} must be in increments of {Step}.", nameof(workers));
        }

        _workers[job] = workers;
    }

    public int TotalAssigned() => _workers.Values.Sum();

    public int SlotsFor(JobType job) => _workers[job] / Step;

    public void SetIdleWorkers(int workers)
    {
        IdleWorkers = workers < 0 ? 0 : workers;
    }
}