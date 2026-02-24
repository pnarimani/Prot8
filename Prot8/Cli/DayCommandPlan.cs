using Prot8.Simulation;

namespace Prot8.Cli.Input;

public sealed class DayCommandPlan(TurnActionChoice action)
{
    public TurnActionChoice Action { get; } = action;
}