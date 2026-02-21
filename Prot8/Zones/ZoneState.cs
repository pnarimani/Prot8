namespace Prot8.Zones;

public sealed class ZoneState
{
    public ZoneState(ZoneId id, string name, int integrity, int capacity, int population)
    {
        Id = id;
        Name = name;
        Integrity = integrity;
        Capacity = capacity;
        Population = population;
    }

    public ZoneId Id { get; }

    public string Name { get; }

    public int Integrity { get; set; }

    public int Capacity { get; set; }

    public int Population { get; set; }

    public bool IsLost { get; set; }

    public override string ToString() => Name;
}