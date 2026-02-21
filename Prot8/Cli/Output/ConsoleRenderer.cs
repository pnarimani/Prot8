using System;
using System.Collections.Generic;
using System.Linq;
using Prot8.Constants;
using Prot8.Jobs;
using Prot8.Laws;
using Prot8.Missions;
using Prot8.Orders;
using Prot8.Simulation;
using Prot8.Zones;

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
        RenderDailyOpportunities(state);
    }

    public void RenderResources(GameState state)
    {
        Console.WriteLine("Resources");
        Console.WriteLine($"  Food: {state.Resources[Resources.ResourceKind.Food],4}  Water: {state.Resources[Resources.ResourceKind.Water],4}  Fuel: {state.Resources[Resources.ResourceKind.Fuel],4}  Medicine: {state.Resources[Resources.ResourceKind.Medicine],4}  Materials: {state.Resources[Resources.ResourceKind.Materials],4}");
        Console.WriteLine($"  Morale: {state.Morale,3}/100  Unrest: {state.Unrest,3}/100  Sickness: {state.Sickness,3}/100");
        Console.WriteLine();
    }

    public void RenderPopulation(GameState state)
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

    public void RenderZones(GameState state)
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

    public void RenderMissions(GameState state)
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

    public void RenderLaws(GameState state)
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

    public void RenderPendingPlan(GameState state, JobAllocation allocation, TurnActionChoice action)
    {
        Console.WriteLine("Pending Day Plan");
        foreach (var job in Enum.GetValues<JobType>())
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
        Console.WriteLine();
        Console.WriteLine("Available Commands");
        Console.WriteLine("  assign <JobType> <Workers>          Set workers for one job (absolute). Workers must be in steps of 5.");
        Console.WriteLine("  clear_assignments                   Reset all job assignments to 0.");
        Console.WriteLine("  enact_law <LawId>                   Queue a law to enact today (replaces any queued optional action).");
        Console.WriteLine("  issue_order <OrderId> [ZoneId]      Queue an emergency order for today (zone required for some orders).");
        Console.WriteLine("  start_mission <MissionId>           Queue a mission to start today.");
        Console.WriteLine("  clear_action                        Remove queued law/order/mission.");
        Console.WriteLine("  show_plan                           Print current pending assignments and queued optional action.");
        Console.WriteLine("  help                                Print this command list.");
        Console.WriteLine("  end_day                             Finalize planning and resolve the day.");
        Console.WriteLine();

        Console.WriteLine("JobType Values");
        foreach (var job in Enum.GetValues<JobType>())
        {
            Console.WriteLine($"  - {job}");
        }

        Console.WriteLine();
        Console.WriteLine("ZoneId Values");
        foreach (var zone in state.Zones)
        {
            Console.WriteLine($"  - {zone.Id} (or {(int)zone.Id})");
        }

        Console.WriteLine();
        Console.WriteLine("LawId Values");
        foreach (var law in LawCatalog.GetAll())
        {
            Console.WriteLine($"  - {law.Id}: {law.Name} | {law.Summary}");
        }

        Console.WriteLine();
        Console.WriteLine("OrderId Values");
        foreach (var order in EmergencyOrderCatalog.GetAll())
        {
            var zoneHint = order.RequiresZoneSelection ? " (requires ZoneId)" : string.Empty;
            Console.WriteLine($"  - {order.Id}: {order.Name}{zoneHint} | {order.Summary}");
        }

        Console.WriteLine();
        Console.WriteLine("MissionId Values");
        foreach (var mission in MissionCatalog.GetAll())
        {
            Console.WriteLine($"  - {mission.Id}: {mission.Name} | {mission.OutcomeHint}");
        }

        Console.WriteLine();
    }

    public void RenderDailyOpportunities(GameState state)
    {
        Console.WriteLine("Available Laws");
        foreach (var law in LawCatalog.GetAll())
        {
            var status = BuildLawStatus(state, law);
            Console.WriteLine($"  - {law.Id}: {law.Name} | {law.Summary} | {status}");
        }

        Console.WriteLine();
        Console.WriteLine("Available Emergency Orders");
        foreach (var order in EmergencyOrderCatalog.GetAll())
        {
            var status = BuildOrderStatus(state, order);
            Console.WriteLine($"  - {order.Id}: {order.Name} | {order.Summary} | {status}");
        }

        Console.WriteLine();
        Console.WriteLine("Available Missions");
        foreach (var mission in MissionCatalog.GetAll())
        {
            var status = mission.CanStart(state, out var reason) ? "Available" : $"Locked: {reason}";
            Console.WriteLine($"  - {mission.Id}: {mission.Name} | {mission.OutcomeHint} | {status}");
        }

        Console.WriteLine();
    }

    public void RenderJobMenu(GameState state)
    {
        Console.WriteLine("Job Allocation (increments of 5 workers)");
        Console.WriteLine("  1. Food Production      Output: +14 food/slot, input: -2 water/slot");
        Console.WriteLine("  2. Water Drawing        Output: +16 water/slot, input: -1 fuel/slot");
        Console.WriteLine("  3. Materials Crafting   Output: +10 materials/slot, input: -2 fuel/slot");
        Console.WriteLine("  4. Repairs              Output: +3 integrity/slot, input: -4 materials, -2 fuel/slot (active perimeter)");
        Console.WriteLine("  5. Clinic Staff         Output: care points + recovery throughput, input: -2 medicine/slot");
        Console.WriteLine("  6. Fuel Scavenging      Output: +8 fuel/slot");
        Console.WriteLine();
    }

    public void RenderActionMenu()
    {
        Console.WriteLine("Choose one optional action for today (max 1):");
        Console.WriteLine("  0. No action");
        Console.WriteLine("  1. Enact Law");
        Console.WriteLine("  2. Issue Emergency Order");
        Console.WriteLine("  3. Start Mission");
    }

    public void RenderLawOptions(GameState state)
    {
        Console.WriteLine("Laws");

        var cooldownActive = state.LastLawDay != int.MinValue
            && state.Day - state.LastLawDay < GameBalance.LawCooldownDays;
        if (cooldownActive)
        {
            Console.WriteLine($"  Law cooldown active. Next law day: {state.LastLawDay + GameBalance.LawCooldownDays}");
        }

        var index = 1;
        foreach (var law in LawCatalog.GetAll())
        {
            var enacted = state.ActiveLawIds.Contains(law.Id);
            var status = "Available";
            if (enacted)
            {
                status = "Already enacted";
            }
            else if (cooldownActive)
            {
                status = "Blocked by law cooldown";
            }
            else if (!law.CanEnact(state, out var reason))
            {
                status = $"Locked: {reason}";
            }

            Console.WriteLine($"  {index}. {law.Name} | {law.Summary} | {status}");
            index += 1;
        }

        Console.WriteLine("  0. Cancel");
        Console.WriteLine();
    }

    public void RenderOrderOptions(GameState state)
    {
        Console.WriteLine("Emergency Orders");
        var index = 1;
        foreach (var order in EmergencyOrderCatalog.GetAll())
        {
            var status = order.CanIssue(state, null, out var reason)
                ? "Available"
                : (order.RequiresZoneSelection ? "Zone selection required" : $"Locked: {reason}");
            Console.WriteLine($"  {index}. {order.Name} | {order.Summary} | {status}");
            index += 1;
        }

        Console.WriteLine("  0. Cancel");
        Console.WriteLine();
    }

    public void RenderMissionOptions(GameState state)
    {
        Console.WriteLine("Missions");
        var index = 1;
        foreach (var mission in MissionCatalog.GetAll())
        {
            var status = mission.CanStart(state, out var reason)
                ? "Available"
                : $"Locked: {reason}";
            Console.WriteLine($"  {index}. {mission.Name} | Duration: {mission.DurationDays} day(s), Crew: {mission.WorkerCost}, Outcome: {mission.OutcomeHint} | {status}");
            index += 1;
        }

        Console.WriteLine("  0. Cancel");
        Console.WriteLine();
    }

    public void RenderZoneSelectionPrompt(string title, IEnumerable<ZoneState> zones)
    {
        Console.WriteLine(title);
        foreach (var zone in zones)
        {
            Console.WriteLine($"  {(int)zone.Id}. {zone.Name} (Integrity {zone.Integrity}, Lost: {zone.IsLost})");
        }

        Console.WriteLine("  0. Cancel");
        Console.WriteLine();
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

    private static string BuildLawStatus(GameState state, ILaw law)
    {
        if (state.ActiveLawIds.Contains(law.Id))
        {
            return "Already enacted";
        }

        var cooldownActive = state.LastLawDay != int.MinValue
            && state.Day - state.LastLawDay < GameBalance.LawCooldownDays;
        if (cooldownActive)
        {
            return $"Locked: law cooldown until Day {state.LastLawDay + GameBalance.LawCooldownDays}";
        }

        return law.CanEnact(state, out var reason) ? "Available" : $"Locked: {reason}";
    }

    private static string BuildOrderStatus(GameState state, IEmergencyOrder order)
    {
        if (!order.RequiresZoneSelection)
        {
            return order.CanIssue(state, null, out var reason) ? "Available" : $"Locked: {reason}";
        }

        foreach (var zone in state.Zones)
        {
            if (order.CanIssue(state, zone.Id, out _))
            {
                return "Available (requires ZoneId)";
            }
        }

        return "Locked: no valid zone target";
    }
}
