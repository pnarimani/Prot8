using Prot8.Simulation;

namespace Prot8.Missions;

public abstract class MissionBase : IMissionDefinition
{
    protected MissionBase(string id, string name, string outcomeHint, int durationDays, int workerCost)
    {
        Id = id;
        Name = name;
        OutcomeHint = outcomeHint;
        DurationDays = durationDays;
        WorkerCost = workerCost;
    }

    public string Id { get; }

    public string Name { get; }

    public string OutcomeHint { get; }

    public virtual string GetDynamicTooltip(GameState state) => OutcomeHint;

    public int DurationDays { get; }

    public int WorkerCost { get; }

    public virtual bool CanStart(GameState state, out string reason)
    {
        if (state.AvailableHealthyWorkersForAllocation < WorkerCost)
        {
            reason = $"Requires {WorkerCost} healthy workers free.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public abstract void ResolveOutcome(GameState state, ActiveMission mission, DayResolutionReport report);

    protected static int RollPercent(GameState state) => state.Random.Next(1, 101);
}