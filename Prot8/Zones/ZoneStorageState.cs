using Prot8.Constants;
using Prot8.Resources;

namespace Prot8.Zones;

public sealed class ZoneStorageState(ZoneId zoneId)
{
    static readonly ResourceKind[] StoredKinds =
    [
        ResourceKind.Food, ResourceKind.Water, ResourceKind.Fuel,
        ResourceKind.Medicine, ResourceKind.Materials
    ];

    readonly Dictionary<ResourceKind, int> _stored = new()
    {
        [ResourceKind.Food] = 0,
        [ResourceKind.Water] = 0,
        [ResourceKind.Fuel] = 0,
        [ResourceKind.Medicine] = 0,
        [ResourceKind.Materials] = 0,
    };

    public ZoneId ZoneId { get; } = zoneId;

    public int UpgradeLevel { get; set; }

    public int Capacity => GameBalance.StorageBaseCapacity + UpgradeLevel * GameBalance.StorageCapacityPerUpgrade;

    public Dictionary<ResourceKind, int>? LostContents { get; private set; }

    public int Add(ResourceKind kind, int amount)
    {
        if (amount <= 0) return 0;
        var current = _stored[kind];
        var space = Capacity - current;
        if (space <= 0) return 0;
        var actual = Math.Min(amount, space);
        _stored[kind] = current + actual;
        return actual;
    }

    public int Consume(ResourceKind kind, int amount)
    {
        if (amount <= 0) return 0;
        var current = _stored[kind];
        var actual = Math.Min(amount, current);
        _stored[kind] = current - actual;
        return actual;
    }

    public int GetStored(ResourceKind kind) => _stored[kind];

    public IReadOnlyDictionary<ResourceKind, int> Snapshot() => _stored;

    public Dictionary<ResourceKind, int> ClearAndRecordLoss()
    {
        var lost = new Dictionary<ResourceKind, int>();
        foreach (var kind in StoredKinds)
        {
            lost[kind] = _stored[kind];
            _stored[kind] = 0;
        }

        LostContents = lost;
        return lost;
    }
}
