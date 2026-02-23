using Prot8.Simulation;

namespace Prot8.Mood;

public sealed class MoodEntry(int priority, Func<GameState, bool> condition, string[] lines)
{
    public int Priority { get; } = priority;
    public Func<GameState, bool> Condition { get; } = condition;
    public string[] Lines { get; } = lines;
}
