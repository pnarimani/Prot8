using Prot8.Simulation;

namespace Prot8.Mood;

public static class MoodSelector
{
    public static string? Select(GameState state)
    {
        var highestPriority = -1;

        foreach (var entry in MoodLines.All)
        {
            if (entry.Priority > highestPriority && entry.Condition(state))
                highestPriority = entry.Priority;
        }

        if (highestPriority < 0)
            return null;

        var pool = new List<string>();

        foreach (var entry in MoodLines.All)
        {
            if (entry.Priority == highestPriority && entry.Condition(state))
                pool.AddRange(entry.Lines);
        }

        if (pool.Count == 0)
            return null;

        return pool[state.Random.Next(pool.Count)];
    }
}
