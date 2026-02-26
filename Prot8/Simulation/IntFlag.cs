namespace Prot8.Simulation;

public sealed class IntFlag
{
    private readonly int _min;
    private readonly int _max;
    private readonly List<PendingExpiry> _expiries = new();

    public int Value { get; private set; }

    public IntFlag(int defaultValue, int min, int max)
    {
        _min = min;
        _max = max;
        Value = Math.Clamp(defaultValue, min, max);
    }

    public void Add(int delta, int? lifetimeDays = null)
    {
        Value = Math.Clamp(Value + delta, _min, _max);
        if (lifetimeDays.HasValue)
            _expiries.Add(new PendingExpiry(delta, lifetimeDays.Value));
    }

    public void TickDay()
    {
        for (int i = _expiries.Count - 1; i >= 0; i--)
        {
            _expiries[i] = _expiries[i] with { DaysRemaining = _expiries[i].DaysRemaining - 1 };
            if (_expiries[i].DaysRemaining <= 0)
            {
                Value = Math.Clamp(Value - _expiries[i].Delta, _min, _max);
                _expiries.RemoveAt(i);
            }
        }
    }

    public static implicit operator int(IntFlag flag) => flag.Value;

    private sealed record PendingExpiry(int Delta, int DaysRemaining);
}
