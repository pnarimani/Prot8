namespace Prot8.Simulation;

public readonly record struct DeltaEntry(string Source, int Value);

public sealed class TrackedDelta
{
    readonly List<DeltaEntry> _entries = [];

    public int Value
    {
        get
        {
            var result = 0;
            foreach (var entry in _entries)
            {
                result += entry.Value;
            }

            return result;
        }
    }

    public IReadOnlyList<DeltaEntry> Entries => _entries;

    public void Add(string source, int amount)
    {
        if (amount != 0) _entries.Add(new(source, amount));
    }
}
