using System;
using System.Linq;
using Prot8.Constants;
using Prot8.Jobs;
using Prot8.Laws;
using Prot8.Missions;
using Prot8.Orders;
using Prot8.Simulation;

namespace Prot8.Cli.Output;

public sealed class ConsoleRenderer
{
    public void RenderDayStart(GameState state)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 76));
        Console.WriteLine($"Day {state.Day} / {GameBalance.TargetSurvivalDay}  |  Siege Intensity: {state.SiegeIntensity}  |  Active Perimeter: {state.ActivePerimeterZone.Name}");
        Console.WriteLine(new string('=', 76));
        Console.WriteLine();

        RenderResources(state);
        RenderPopulation(state);
        RenderZones(state);
        RenderMissions(state);
        RenderLaws(state);
        RenderActionReference(state);
    }

    public void RenderPendingPlan(GameState state, JobAllocation allocation, TurnActionChoice action)
    {
        Console.WriteLine("Pending Day Plan");
        foreach (var job in ActionAvailability.GetJobTypes())
        {
            Console.WriteLine($"  {job}: {allocation.Workers[job]} workers");
        }

        Console.WriteLine($"  Total assigned: {allocation.TotalAssigned()} / {state.AvailableHealthyWorkersForAllocation}");
        Console.WriteLine($"  Idle workers: {allocation.IdleWorkers}");

        if (!action.HasAction)
        {
            Console.WriteLine("  Queued optional action: none");
        }
        else if (!string.IsNullOrWhiteSpace(action.LawId))
        {
            var law = LawCatalog.Find(action.LawId);
            Console.WriteLine($"  Queued optional action: Law -> {law?.Name ?? action.LawId}");
        }
        else if (!string.IsNullOrWhiteSpace(action.EmergencyOrderId))
        {
            var order = EmergencyOrderCatalog.Find(action.EmergencyOrderId);
            var zoneSuffix = action.SelectedZoneForOrder.HasValue ? $" ({action.SelectedZoneForOrder.Value})" : string.Empty;
            Console.WriteLine($"  Queued optional action: Emergency Order -> {order?.Name ?? action.EmergencyOrderId}{zoneSuffix}");
        }
        else if (!string.IsNullOrWhiteSpace(action.MissionId))
        {
            var mission = MissionCatalog.Find(action.MissionId);
            Console.WriteLine($"  Queued optional action: Mission -> {mission?.Name ?? action.MissionId}");
        }

        Console.WriteLine();
    }

    public void RenderActionReference(GameState state)
    {
        Console.WriteLine("Available Actions");
        Console.WriteLine("  assign <JobRef|JobType> <Workers>  Set workers for one production slot (absolute value, steps of 5).");
        Console.WriteLine("  enact <LawRef|LawId>               Queue one available law for today.");
        Console.WriteLine("  order <OrderRef|OrderId> [ZoneId]  Queue one available emergency order for today.");
        Console.WriteLine("  mission <MissionRef|MissionId>     Queue one available mission for today.");
        Console.WriteLine("  clear_assignments                   Reset all job assignments to 0.");
        Console.WriteLine("  clear_action                        Clear queued law/order/mission action.");
        Console.WriteLine("  show_plan                           Print current pending assignments and queued action.");
        Console.WriteLine("  help                                Print this available-actions list.");
        Console.WriteLine("  end_day                             Resolve simulation using current plan.");
        Console.WriteLine();

        RenderAvailableJobs();
        RenderAvailableLaws(state);
        RenderAvailableOrders(state);
        RenderAvailableMissions(state);
    }

    public void RenderDayReport(GameState state, DayResolutionReport report)
    {
        Console.WriteLine();
        Console.WriteLine($"Day {report.Day} Resolution");
        Console.WriteLine(new string('-', 76));

        foreach (var entry in report.Entries)
        {
            Console.WriteLine($"[{entry.Tag}] {entry.Message}");
        }

        if (report.TriggeredEvents.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Triggered Events");
            foreach (var evt in report.TriggeredEvents)
            {
                Console.WriteLine($"  - {evt}");
            }
        }

        if (report.ResolvedMissions.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Mission Outcomes");
            foreach (var mission in report.ResolvedMissions)
            {
                Console.WriteLine($"  - {mission}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("Recovery Status");
        Console.WriteLine($"  Recovery enabled: {(report.RecoveryEnabledToday ? "Yes" : "No")}");
        Console.WriteLine($"  Queue size: {state.Population.RecoveryQueue.Sum(c => c.Count)}");
        Console.WriteLine($"  Recovered today: {report.RecoveredWorkersToday}");
        Console.WriteLine($"  Medicine spent on recovery: {report.RecoveryMedicineSpentToday}");
        if (!string.IsNullOrWhiteSpace(report.RecoveryBlockedReason))
        {
            Console.WriteLine($"  Blocked reason: {report.RecoveryBlockedReason}");
        }

        Console.WriteLine();
        Console.WriteLine($"End of Day {report.Day}: Food {state.Resources[Resources.ResourceKind.Food]}, Water {state.Resources[Resources.ResourceKind.Water]}, Fuel {state.Resources[Resources.ResourceKind.Fuel]}, Meds {state.Resources[Resources.ResourceKind.Medicine]}, Materials {state.Resources[Resources.ResourceKind.Materials]}");
        Console.WriteLine($"Morale {state.Morale}, Unrest {state.Unrest}, Sickness {state.Sickness}, Siege Intensity {state.SiegeIntensity}");
    }

    public void RenderFinal(GameState state)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 76));
        if (state.Survived)
        {
            Console.WriteLine($"You endured to Day {GameBalance.TargetSurvivalDay}. The city survives, but at great cost.");
        }
        else
        {
            Console.WriteLine($"Game Over on Day {state.Day}: {state.GameOverCause}");
            if (!string.IsNullOrWhiteSpace(state.GameOverDetails))
            {
                Console.WriteLine(state.GameOverDetails);
            }
        }

        Console.WriteLine($"Total deaths: {state.TotalDeaths}, total desertions: {state.TotalDesertions}");
        Console.WriteLine($"Final Morale: {state.Morale}, Unrest: {state.Unrest}, Sickness: {state.Sickness}");
        Console.WriteLine(new string('=', 76));
        Console.WriteLine();
    }

    private static void RenderResources(GameState state)
    {
        Console.WriteLine("Resources");
        Console.WriteLine($"  Food: {state.Resources[Resources.ResourceKind.Food],4}  Water: {state.Resources[Resources.ResourceKind.Water],4}  Fuel: {state.Resources[Resources.ResourceKind.Fuel],4}  Medicine: {state.Resources[Resources.ResourceKind.Medicine],4}  Materials: {state.Resources[Resources.ResourceKind.Materials],4}");
        Console.WriteLine($"  Morale: {state.Morale,3}/100  Unrest: {state.Unrest,3}/100  Sickness: {state.Sickness,3}/100");
        Console.WriteLine();
    }

    private static void RenderPopulation(GameState state)
    {
        Console.WriteLine("Population");
        Console.WriteLine($"  Healthy Workers: {state.Population.HealthyWorkers}");
        Console.WriteLine($"  Guards:          {state.Population.Guards}");
        Console.WriteLine($"  Sick Workers:    {state.Population.SickWorkers}");
        Console.WriteLine($"  Elderly:         {state.Population.Elderly}");
        Console.WriteLine($"  Total:           {state.Population.TotalPopulation}");
        Console.WriteLine($"  Workers reserved on missions: {state.ReservedWorkersForMissions}");
        Console.WriteLine($"  Available for assignment today: {state.AvailableHealthyWorkersForAllocation}");
        Console.WriteLine();
    }

    private static void RenderZones(GameState state)
    {
        Console.WriteLine("Zones");
        foreach (var zone in state.Zones)
        {
            var status = zone.IsLost ? "LOST" : "ACTIVE";
            var over = zone.Population - zone.Capacity;
            var overText = over > 0 ? $" (Over by {over})" : string.Empty;
            Console.WriteLine($"  {(int)zone.Id}. {zone.Name,-18} Integrity: {zone.Integrity,3}  Capacity: {zone.Capacity,3}  Housed: {zone.Population,3}  {status}{overText}");
        }

        Console.WriteLine();
    }

    private static void RenderMissions(GameState state)
    {
        Console.WriteLine("Active Missions");
        if (state.ActiveMissions.Count == 0)
        {
            Console.WriteLine("  None");
        }
        else
        {
            foreach (var mission in state.ActiveMissions)
            {
                Console.WriteLine($"  {mission.MissionName}: {mission.DaysRemaining} day(s) remaining, {mission.WorkerCost} workers committed");
            }
        }

        Console.WriteLine();
    }

    private static void RenderLaws(GameState state)
    {
        Console.WriteLine("Enacted Laws");
        if (state.ActiveLawIds.Count == 0)
        {
            Console.WriteLine("  None");
        }
        else
        {
            foreach (var lawId in state.ActiveLawIds)
            {
                var law = LawCatalog.Find(lawId);
                Console.WriteLine($"  {law?.Name ?? lawId}");
            }
        }

        Console.WriteLine();
    }

    private static void RenderAvailableJobs()
    {
        Console.WriteLine("Assignable Job Slots");
        var jobs = ActionAvailability.GetJobTypes();
        for (var index = 0; index < jobs.Count; index++)
        {
            var job = jobs[index];
            Console.WriteLine($"  j{index + 1}: {job} | {JobDescription(job)}");
        }

        Console.WriteLine();
    }

    private static void RenderAvailableLaws(GameState state)
    {
        Console.WriteLine("Available Laws");
        var available = ActionAvailability.GetAvailableLaws(state);
        if (available.Count == 0)
        {
            Console.WriteLine("  None currently available.");
            Console.WriteLine();
            return;
        }

        for (var index = 0; index < available.Count; index++)
        {
            var law = available[index];
            Console.WriteLine($"  l{index + 1}: {law.Name} ({law.Id}) | {law.Summary}");
        }

        Console.WriteLine();
    }

    private static void RenderAvailableOrders(GameState state)
    {
        Console.WriteLine("Available Emergency Orders");
        var available = ActionAvailability.GetAvailableOrders(state);
        if (available.Count == 0)
        {
            Console.WriteLine("  None currently available.");
            Console.WriteLine();
            return;
        }

        for (var index = 0; index < available.Count; index++)
        {
            var order = available[index];
            if (!order.RequiresZoneSelection)
            {
                Console.WriteLine($"  o{index + 1}: {order.Name} ({order.Id}) | {order.Summary}");
                continue;
            }

            var validZones = ActionAvailability.GetValidZonesForOrder(state, order);
            var zoneList = validZones.Count == 0 ? "none" : string.Join(", ", validZones);
            Console.WriteLine($"  o{index + 1}: {order.Name} ({order.Id}) | {order.Summary} | valid ZoneId: {zoneList}");
        }

        Console.WriteLine();
    }

    private static void RenderAvailableMissions(GameState state)
    {
        Console.WriteLine("Available Missions");
        var available = ActionAvailability.GetAvailableMissions(state);
        if (available.Count == 0)
        {
            Console.WriteLine("  None currently available.");
            Console.WriteLine();
            return;
        }

        for (var index = 0; index < available.Count; index++)
        {
            var mission = available[index];
            Console.WriteLine($"  m{index + 1}: {mission.Name} ({mission.Id}) | {mission.OutcomeHint}");
        }

        Console.WriteLine();
    }

    private static string JobDescription(JobType job)
    {
        return job switch
        {
            JobType.FoodProduction => "Produce food (+14/slot), consumes water (-2/slot).",
            JobType.WaterDrawing => "Draw water (+16/slot), consumes fuel (-1/slot).",
            JobType.MaterialsCrafting => "Craft materials (+10/slot), consumes fuel (-2/slot).",
            JobType.Repairs => "Repair active perimeter integrity (+3/slot), consumes materials and fuel.",
            JobType.ClinicStaff => "Provide care, reduce sickness pressure, and enable recoveries.",
            JobType.FuelScavenging => "Scavenge fuel (+8/slot).",
            _ => "No description available."
        };
    }
}