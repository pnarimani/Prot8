namespace Prot8.Zones;

public sealed class ZoneState(ZoneId id, string name, int integrity, int capacity)
{
    public ZoneId Id { get; } = id;

    public string Name { get; } = name;

    public int Integrity { get; set; } = integrity;

    public int Capacity { get; set; } = capacity;

    public bool IsLost { get; set; }
    public override string ToString()
    {
        return Name;
    }
}