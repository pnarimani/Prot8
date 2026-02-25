using Prot8.Zones;

namespace Prot8.Buildings;

public sealed class BuildingAllocation(IReadOnlyList<BuildingState> buildings)
{
    public void SetWorkers(BuildingId id, int workers)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(workers);

        var building = Get(id);
        if (building.IsDestroyed)
            throw new InvalidOperationException($"Cannot assign workers to destroyed building {building.Name}.");
        if (workers > building.MaxWorkers)
            throw new ArgumentOutOfRangeException(nameof(workers),
                $"Cannot assign {workers} workers to {building.Name} (max {building.MaxWorkers}).");

        building.AssignedWorkers = workers;
    }

    public int TotalAssigned()
    {
        var sum = 0;
        foreach (var b in buildings)
            sum += b.AssignedWorkers;
        return sum;
    }

    public void Clear()
    {
        foreach (var b in buildings)
        {
            if (!b.IsDestroyed)
                b.AssignedWorkers = 0;
        }
    }

    public int DestroyBuildingsInZone(ZoneId zone)
    {
        var freed = 0;
        foreach (var b in buildings)
        {
            if (b.Zone == zone && !b.IsDestroyed)
            {
                freed += b.AssignedWorkers;
                b.AssignedWorkers = 0;
                b.IsDestroyed = true;
            }
        }
        return freed;
    }

    public int RemoveWorkersProportionally(int count)
    {
        if (count <= 0)
            return 0;

        var total = TotalAssigned();
        if (total == 0)
            return 0;

        var removed = 0;
        foreach (var b in buildings)
        {
            if (b.AssignedWorkers <= 0)
                continue;

            var proportionalShare = (int)Math.Round((double)b.AssignedWorkers / total * count);
            var toRemove = Math.Min(proportionalShare, Math.Min(b.AssignedWorkers, count - removed));
            b.AssignedWorkers -= toRemove;
            removed += toRemove;
        }

        if (removed < count && total > 0)
        {
            var remaining = count - removed;
            foreach (var b in buildings)
            {
                if (remaining <= 0)
                    break;

                var toRemove = Math.Min(remaining, b.AssignedWorkers);
                b.AssignedWorkers -= toRemove;
                removed += toRemove;
                remaining -= toRemove;
            }
        }

        return removed;
    }

    BuildingState Get(BuildingId id)
    {
        foreach (var b in buildings)
        {
            if (b.Id == id)
                return b;
        }
        throw new KeyNotFoundException($"Building not found: {id}");
    }
}
