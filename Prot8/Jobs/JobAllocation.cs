namespace Prot8.Jobs;

public sealed class JobAllocation
{
    readonly Dictionary<JobType, int> _workers = Enum.GetValues<JobType>().ToDictionary(job => job, _ => 0);

    public int IdleWorkers { get; private set; }

    public IReadOnlyDictionary<JobType, int> Workers => _workers;

    public void SetWorkers(JobType job, int workers)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(workers);

        _workers[job] = workers;
    }

    public int TotalAssigned()
    {
        return _workers.Values.Sum();
    }

    public void SetIdleWorkers(int workers)
    {
        IdleWorkers = workers < 0 ? 0 : workers;
    }
}