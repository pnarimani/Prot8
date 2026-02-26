namespace Prot8.Simulation;

public sealed class FlagState
{
    // ── Tyranny Path ──
    public IntFlag Tyranny { get; } = new(0, min: 0, max: 10);
    public BoolFlag IronFist { get; } = new();
    public BoolFlag MartialState { get; } = new();
    public BoolFlag MercyDenied { get; } = new();

    // ── Faith Path ──
    public IntFlag Faith { get; } = new(0, min: 0, max: 10);
    public BoolFlag FaithRisen { get; } = new();
    public BoolFlag PeopleFirst { get; } = new();

    // ── Fortification Path ──
    public IntFlag Fortification { get; } = new(0, min: 0, max: 10);
    public BoolFlag GarrisonState { get; } = new();
    public BoolFlag WallsHold { get; } = new();

    // ── Laws ──
    public BoolFlag CannibalismEnacted { get; } = new();

    // ── Shared ──
    public IntFlag FearLevel { get; } = new(0, min: 0, max: 5);

    public void TickDay()
    {
        Tyranny.TickDay();
        Faith.TickDay();
        Fortification.TickDay();
        FearLevel.TickDay();
    }
}
