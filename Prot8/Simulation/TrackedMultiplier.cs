namespace Prot8.Simulation;

public readonly record struct MultiplierEntry(string Source, double Value);

public sealed class TrackedMultiplier
{
    readonly List<MultiplierEntry> _entries = [];

    public double Value
    {
        get
        {
            var result = 1.0;
            foreach (var entry in _entries)
            {
                result *= entry.Value;
            }

            return result;
        }
    }

    public IReadOnlyList<MultiplierEntry> Entries => _entries;

    public void Apply(string source, double multiplier) => _entries.Add(new(source, multiplier));
}
