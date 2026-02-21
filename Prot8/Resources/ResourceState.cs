using System.Collections.Generic;

namespace Prot8.Resources;

public sealed class ResourceState
{
    private readonly Dictionary<ResourceKind, int> _values;

    public ResourceState(int food, int water, int fuel, int medicine, int materials)
    {
        _values = new Dictionary<ResourceKind, int>
        {
            [ResourceKind.Food] = food,
            [ResourceKind.Water] = water,
            [ResourceKind.Fuel] = fuel,
            [ResourceKind.Medicine] = medicine,
            [ResourceKind.Materials] = materials
        };
    }

    public int this[ResourceKind kind]
    {
        get => _values[kind];
        set => _values[kind] = value < 0 ? 0 : value;
    }

    public IReadOnlyDictionary<ResourceKind, int> Snapshot() => _values;

    public int Consume(ResourceKind kind, int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        var current = _values[kind];
        var consumed = amount > current ? current : amount;
        _values[kind] = current - consumed;
        return consumed;
    }

    public int Add(ResourceKind kind, int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        _values[kind] += amount;
        return amount;
    }

    public bool Has(ResourceKind kind, int amount) => _values[kind] >= amount;
}