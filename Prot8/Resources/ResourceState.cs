using Prot8.Constants;
using Prot8.Zones;

namespace Prot8.Resources;

public sealed class ResourceState
{
    static readonly ResourceKind[] StoredKinds =
    [
        ResourceKind.Food, ResourceKind.Water, ResourceKind.Fuel,
        ResourceKind.Medicine, ResourceKind.Materials
    ];

    // Zones ordered safest-first (Keep..OuterFarms) for Add
    readonly List<ZoneStorageState> _zoneStorages;

    // Zones ordered least-safe-first (OuterFarms..Keep) for Consume
    readonly List<ZoneStorageState> _consumeOrder;

    public ResourceState(IReadOnlyList<ZoneStorageState> zoneStorages)
    {
        // Store safest-first order (highest ZoneId = Keep = safest)
        _zoneStorages = zoneStorages.OrderByDescending(z => z.ZoneId).ToList();
        // Consume order: least safe first (lowest ZoneId = OuterFarms)
        _consumeOrder = zoneStorages.OrderBy(z => z.ZoneId).ToList();
    }

    public IReadOnlyList<ZoneStorageState> ZoneStorages => _zoneStorages;

    public int this[ResourceKind kind]
    {
        get
        {
            var total = 0;
            foreach (var zs in _zoneStorages)
            {
                if (zs.LostContents is null)
                    total += zs.GetStored(kind);
            }
            return total;
        }
        set
        {
            var current = this[kind];
            var newValue = value < 0 ? 0 : value;
            var delta = newValue - current;
            if (delta > 0)
                Add(kind, delta);
            else if (delta < 0)
                Consume(kind, -delta);
        }
    }

    public IReadOnlyDictionary<ResourceKind, int> Snapshot()
    {
        var result = new Dictionary<ResourceKind, int>();
        foreach (var kind in StoredKinds)
        {
            result[kind] = this[kind];
        }
        return result;
    }

    public int Consume(ResourceKind kind, int amount)
    {
        if (amount <= 0) return 0;

        var totalConsumed = 0;
        var remaining = amount;
        // Drain from least safe first
        foreach (var zs in _consumeOrder)
        {
            if (zs.LostContents is not null) continue;
            if (remaining <= 0) break;
            var consumed = zs.Consume(kind, remaining);
            totalConsumed += consumed;
            remaining -= consumed;
        }
        return totalConsumed;
    }

    public int Add(ResourceKind kind, int amount)
    {
        if (amount <= 0) return 0;

        var totalAdded = 0;
        var remaining = amount;
        // Fill safest first
        foreach (var zs in _zoneStorages)
        {
            if (zs.LostContents is not null) continue;
            if (remaining <= 0) break;
            var added = zs.Add(kind, remaining);
            totalAdded += added;
            remaining -= added;
        }
        return totalAdded;
    }

    public bool Has(ResourceKind kind, int amount) => this[kind] >= amount;

    public ZoneStorageState GetZoneStorage(ZoneId zoneId)
    {
        foreach (var zs in _zoneStorages)
        {
            if (zs.ZoneId == zoneId) return zs;
        }
        throw new KeyNotFoundException($"Zone storage not found: {zoneId}");
    }

    public Dictionary<ResourceKind, int> DestroyZoneStorage(ZoneId zoneId)
    {
        var zs = GetZoneStorage(zoneId);
        return zs.ClearAndRecordLoss();
    }

    public void SalvageToNextZone(ZoneId lostZoneId, double percent)
    {
        if (percent <= 0) return;

        var lostStorage = GetZoneStorage(lostZoneId);

        // Find the next inner (safer) zone that isn't lost
        ZoneStorageState? target = null;
        foreach (var zs in _zoneStorages)
        {
            if (zs.LostContents is not null) continue;
            if (zs.ZoneId > lostZoneId)
            {
                target = zs;
                break;
            }
        }

        // Fallback: find any non-lost zone
        if (target is null)
        {
            foreach (var zs in _zoneStorages)
            {
                if (zs.LostContents is null && zs.ZoneId != lostZoneId)
                {
                    target = zs;
                    break;
                }
            }
        }

        if (target is null) return;

        foreach (var kind in StoredKinds)
        {
            var stored = lostStorage.GetStored(kind);
            var salvageAmount = (int)Math.Floor(stored * percent);
            if (salvageAmount > 0)
            {
                target.Add(kind, salvageAmount);
            }
        }
    }

    public int GetTotalCapacity(ResourceKind kind)
    {
        var total = 0;
        foreach (var zs in _zoneStorages)
        {
            if (zs.LostContents is null)
                total += zs.Capacity;
        }
        return total;
    }

    public int GetAvailableSpace(ResourceKind kind)
    {
        return GetTotalCapacity(kind) - this[kind];
    }
}
