using Prot8.Constants;
using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Buildings;

public static class WorkerAllocationStrategy
{
    public static void ApplyAutomaticAllocation(GameState state)
    {
        switch (GameBalance.AllocationMode)
        {
            case WorkerAllocationMode.ManualAssignment:
                return;
            case WorkerAllocationMode.AutoAllocation:
                ApplyAutoAllocation(state);
                break;
            case WorkerAllocationMode.PriorityQueue:
                ApplyPriorityAllocation(state);
                break;
            case WorkerAllocationMode.BuildingActivation:
                ApplyActivationAllocation(state);
                break;
        }
    }

    static void ApplyAutoAllocation(GameState state)
    {
        var available = state.AvailableHealthyWorkersForAllocation;
        var eligible = new List<BuildingState>();
        var totalDemand = 0;

        foreach (var b in state.Buildings)
        {
            if (!b.IsDestroyed)
            {
                eligible.Add(b);
                totalDemand += b.MaxWorkers;
            }
            else
            {
                b.AssignedWorkers = 0;
            }
        }

        DistributeProportionally(eligible, available, totalDemand);
    }

    static void ApplyPriorityAllocation(GameState state)
    {
        foreach (var b in state.Buildings)
            b.AssignedWorkers = 0;

        var remaining = state.AvailableHealthyWorkersForAllocation;

        foreach (var resource in state.ResourcePriority)
        {
            foreach (var b in state.Buildings)
            {
                if (b.IsDestroyed || remaining <= 0)
                    continue;

                if (b.Outputs.Count == 0 || b.Outputs[0].Resource != resource)
                    continue;

                var assign = Math.Min(b.MaxWorkers, remaining);
                b.AssignedWorkers = assign;
                remaining -= assign;
            }
        }
    }

    static void ApplyActivationAllocation(GameState state)
    {
        var available = state.AvailableHealthyWorkersForAllocation;
        var eligible = new List<BuildingState>();
        var totalDemand = 0;

        foreach (var b in state.Buildings)
        {
            if (!b.IsDestroyed && b.IsActive)
            {
                eligible.Add(b);
                totalDemand += b.MaxWorkers;
            }
            else
            {
                b.AssignedWorkers = 0;
            }
        }

        DistributeProportionally(eligible, available, totalDemand);
    }

    static void DistributeProportionally(List<BuildingState> buildings, int available, int totalDemand)
    {
        if (totalDemand == 0 || buildings.Count == 0)
            return;

        if (available >= totalDemand)
        {
            foreach (var b in buildings)
                b.AssignedWorkers = b.MaxWorkers;
            return;
        }

        var assigned = 0;
        foreach (var b in buildings)
        {
            var share = (int)Math.Floor((double)b.MaxWorkers * available / totalDemand);
            b.AssignedWorkers = share;
            assigned += share;
        }

        // Distribute remainder by definition order
        var remainder = available - assigned;
        foreach (var b in buildings)
        {
            if (remainder <= 0)
                break;
            var gap = b.MaxWorkers - b.AssignedWorkers;
            if (gap > 0)
            {
                var extra = Math.Min(gap, remainder);
                b.AssignedWorkers += extra;
                remainder -= extra;
            }
        }
    }
}
