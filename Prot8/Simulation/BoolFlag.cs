namespace Prot8.Simulation;

public sealed class BoolFlag
{
    public bool Value { get; private set; }

    public BoolFlag(bool defaultValue = false) => Value = defaultValue;

    public void Set(bool value = true) => Value = value;

    public static implicit operator bool(BoolFlag flag) => flag.Value;
}
