namespace Prot8.Jobs;

public sealed class JobAllocation
{
    readonly Dictionary<JobType, int> _workers = Enum.GetValues<JobType>().ToDictionary(job => job, _ => 0);

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

    public void Clear()
    {
        foreach (var kvp in _workers)
            _workers[kvp.Key] = 0;
    }

    public int RemoveWorkersProportionally(int count)
    {
        if (count <= 0)
        {
            return 0;
        }

        var total = TotalAssigned();
        if (total == 0)
        {
            return 0;
        }

        var removed = 0;
        foreach (var job in _workers.Keys.ToList())
        {
            var jobWorkers = _workers[job];
            if (jobWorkers <= 0)
            {
                continue;
            }

            var proportionalShare = (int)Math.Round((double)jobWorkers / total * count);
            var toRemove = Math.Min(proportionalShare, jobWorkers);
            _workers[job] -= toRemove;
            removed += toRemove;
        }

        if (removed < count && total > 0)
        {
            var remaining = count - removed;
            foreach (var job in _workers.Keys.ToList())
            {
                if (remaining <= 0)
                {
                    break;
                }

                var toRemove = Math.Min(remaining, _workers[job]);
                _workers[job] -= toRemove;
                removed += toRemove;
                remaining -= toRemove;
            }
        }

        return removed;
    }
}