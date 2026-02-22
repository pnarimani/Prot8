using System;
using System.IO;
using System.Linq;
using Prot8.Constants;
using Prot8.Events;
using Prot8.Jobs;
using Prot8.Laws;
using Prot8.Missions;
using Prot8.Orders;
using Prot8.Simulation;

namespace Prot8.Cli.Output;

public sealed class ConsoleRenderer
{
    private readonly TextWriter _out;
    private readonly bool _noShortcuts;

    public ConsoleRenderer(TextWriter output, bool noShortcuts)
    {
        _out = output;
        _noShortcuts = noShortcuts;
    }

    public void RenderDayStart(GameState state)
    {
        _out.WriteLine();
        // _out.WriteLine(new string('=', 76));
        _out.WriteLine($"Day {state.Day} / {GameBalance.TargetSurvivalDay}  |  Siege Intensity: {state.SiegeIntensity}  |  Active Perimeter: {state.ActivePerimeterZone.Name}");
        // _out.WriteLine(new string('=', 76));
        _out.WriteLine();

        RenderResources(state);
        RenderPopulation(state);
        RenderJobAssignments(state);
        RenderZones(state);
        RenderMissions(state);
        RenderLaws(state);
        RenderEvents();
        RenderActionReference(state);
    }

    public void RenderPendingPlan(GameState state, JobAllocation allocation, TurnActionChoice action)
    {
        _out.WriteLine("Pending Day Plan");
        foreach (var job in ActionAvailability.GetJobTypes())
        {
            _out.WriteLine($"  {job}: {allocation.Workers[job]} workers");
        }

        _out.WriteLine($"  Total assigned: {allocation.TotalAssigned()} / {state.AvailableHealthyWorkersForAllocation}");
        _out.WriteLine($"  Idle workers: {allocation.IdleWorkers}");

        if (!action.HasAction)
        {
            _out.WriteLine("  Queued optional action: none");
        }
        else if (!string.IsNullOrWhiteSpace(action.LawId))
        {
            var law = LawCatalog.Find(action.LawId);
            _out.WriteLine($"  Queued optional action: Law -> {law?.Name ?? action.LawId}");
        }
        else if (!string.IsNullOrWhiteSpace(action.EmergencyOrderId))
        {
            var order = EmergencyOrderCatalog.Find(action.EmergencyOrderId);
            var zoneSuffix = action.SelectedZoneForOrder.HasValue ? $" ({action.SelectedZoneForOrder.Value})" : string.Empty;
            _out.WriteLine($"  Queued optional action: Emergency Order -> {order?.Name ?? action.EmergencyOrderId}{zoneSuffix}");
        }
        else if (!string.IsNullOrWhiteSpace(action.MissionId))
        {
            var mission = MissionCatalog.Find(action.MissionId);
            _out.WriteLine($"  Queued optional action: Mission -> {mission?.Name ?? action.MissionId}");
        }

        _out.WriteLine();
    }

    public void RenderActionReference(GameState state)
    {
        RenderAvailableJobs();
        RenderAvailableLaws(state);
        RenderAvailableOrders(state);
        RenderAvailableMissions(state);
        
        _out.WriteLine();
        _out.WriteLine("Available Commands (sections with < > are mandatory)");
        var jobRef = _noShortcuts ? "JobType" : "JobRef|JobType";
        var lawRef = _noShortcuts ? "LawId" : "LawRef|LawId";
        var orderRef = _noShortcuts ? "OrderId" : "OrderRef|OrderId";
        var missionRef = _noShortcuts ? "MissionId" : "MissionRef|MissionId";
        _out.WriteLine($"  assign <{jobRef}> <Workers>   Set workers for one production slot (absolute value, steps of 5).");
        _out.WriteLine($"  enact <{lawRef}>                Queue one available law for today.");
        _out.WriteLine($"  order <{orderRef}> [ZoneId]   Queue one available emergency order for today.");
        _out.WriteLine($"  mission <{missionRef}>      Queue one available mission for today.");
        _out.WriteLine("  clear_assignments                   Reset all job assignments to 0.");
        _out.WriteLine("  clear_action                        Clear queued law/order/mission action.");
        _out.WriteLine("  show_plan                           Print current pending assignments and queued action.");
        _out.WriteLine("  help                                Print this available-actions list.");
        _out.WriteLine("  end_day                             Resolve simulation using current plan.");
        _out.WriteLine();
    }

    public void RenderDayReport(GameState state, DayResolutionReport report)
    {
        _out.WriteLine();
        _out.WriteLine($"Day {report.Day} Resolution");
        _out.WriteLine(new string('-', 76));

        foreach (var entry in report.Entries)
        {
            _out.WriteLine($"[{entry.Tag}] {entry.Message}");
        }

        if (report.TriggeredEvents.Count > 0)
        {
            _out.WriteLine();
            _out.WriteLine("Triggered Events");
            foreach (var evt in report.TriggeredEvents)
            {
                _out.WriteLine($"  - {evt}");
            }
        }

        if (report.ResolvedMissions.Count > 0)
        {
            _out.WriteLine();
            _out.WriteLine("Mission Outcomes");
            foreach (var mission in report.ResolvedMissions)
            {
                _out.WriteLine($"  - {mission}");
            }
        }

        _out.WriteLine();
        _out.WriteLine("Recovery Status");
        _out.WriteLine($"  Recovery enabled: {(report.RecoveryEnabledToday ? "Yes" : "No")}");
        _out.WriteLine($"  Queue size: {state.Population.RecoveryQueue.Sum(c => c.Count)}");
        _out.WriteLine($"  Recovered today: {report.RecoveredWorkersToday}");
        _out.WriteLine($"  Medicine spent on recovery: {report.RecoveryMedicineSpentToday}");
        if (!string.IsNullOrWhiteSpace(report.RecoveryBlockedReason))
        {
            _out.WriteLine($"  Blocked reason: {report.RecoveryBlockedReason}");
        }

        _out.WriteLine();
        _out.WriteLine($"End of Day {report.Day}: Food {state.Resources[Resources.ResourceKind.Food]}, Water {state.Resources[Resources.ResourceKind.Water]}, Fuel {state.Resources[Resources.ResourceKind.Fuel]}, Meds {state.Resources[Resources.ResourceKind.Medicine]}, Materials {state.Resources[Resources.ResourceKind.Materials]}");
        _out.WriteLine($"Morale {state.Morale}, Unrest {state.Unrest}, Sickness {state.Sickness}, Siege Intensity {state.SiegeIntensity}");
    }

    public void RenderFinal(GameState state)
    {
        _out.WriteLine();
        _out.WriteLine(new string('=', 76));
        if (state.Survived)
        {
            _out.WriteLine($"You endured to Day {GameBalance.TargetSurvivalDay}. The city survives, but at great cost.");
        }
        else
        {
            _out.WriteLine($"Game Over on Day {state.Day}: {state.GameOverCause}");
            if (!string.IsNullOrWhiteSpace(state.GameOverDetails))
            {
                _out.WriteLine(state.GameOverDetails);
            }
        }

        _out.WriteLine($"Total deaths: {state.TotalDeaths}, total desertions: {state.TotalDesertions}");
        _out.WriteLine($"Final Morale: {state.Morale}, Unrest: {state.Unrest}, Sickness: {state.Sickness}");
        _out.WriteLine(new string('=', 76));
        _out.WriteLine();
    }

    private void RenderResources(GameState state)
    {
        var pop = state.Population.TotalPopulation;
        var foodNeed = (int)Math.Ceiling(pop * GameBalance.FoodPerPersonPerDay);
        var waterNeed = (int)Math.Ceiling(pop * GameBalance.WaterPerPersonPerDay);
        var fuelNeed = (int)Math.Ceiling(pop * GameBalance.FuelPerPersonPerDay);

        _out.WriteLine("Resources");
        _out.WriteLine($"  Food: {state.Resources[Resources.ResourceKind.Food],4}  Water: {state.Resources[Resources.ResourceKind.Water],4}  Fuel: {state.Resources[Resources.ResourceKind.Fuel],4}  Medicine: {state.Resources[Resources.ResourceKind.Medicine],4}  Materials: {state.Resources[Resources.ResourceKind.Materials],4}");
        _out.WriteLine($"  Daily need ({pop} pop):  Food ~{foodNeed}  Water ~{waterNeed}  Fuel ~{fuelNeed}  [shortfall each day → +Unrest  −Morale  +Sickness]");
        _out.WriteLine($"  Morale: {state.Morale,3}/100  Unrest: {state.Unrest,3}/100  Sickness: {state.Sickness,3}/100  {SicknessStatusNote(state.Sickness)}");
        _out.WriteLine();
    }

    private static string SicknessStatusNote(int sickness)
    {
        if (sickness > 70)
            return "[recovery LOCKED | deaths each day]";
        if (sickness >= 40)
            return $"[recovery LOCKED at ≥40]";
        return "[recovery enabled]";
    }

    private void RenderPopulation(GameState state)
    {
        _out.WriteLine("Population");
        _out.WriteLine($"  Healthy Workers: {state.Population.HealthyWorkers}");
        _out.WriteLine($"  Guards:          {state.Population.Guards}");
        _out.WriteLine($"  Sick Workers:    {state.Population.SickWorkers}");
        _out.WriteLine($"  Elderly:         {state.Population.Elderly}");
        _out.WriteLine($"  Total:           {state.Population.TotalPopulation}");
        _out.WriteLine($"  Workers reserved on missions: {state.ReservedWorkersForMissions}");
        _out.WriteLine($"  Available for assignment today: {state.AvailableHealthyWorkersForAllocation}");
        _out.WriteLine();
    }

    private void RenderJobAssignments(GameState state)
    {
        var multiplier = StatModifiers.ComputeGlobalProductionMultiplier(state);
        _out.WriteLine($"Job Assignments  (production ×{multiplier:F2} from morale/unrest/sickness)");

        var jobs = ActionAvailability.GetJobTypes();
        for (var i = 0; i < jobs.Count; i++)
        {
            var job = jobs[i];
            var workers = state.Allocation.Workers[job];
            var slots = state.Allocation.SlotsFor(job);

            string outputDesc;
            if (slots == 0)
            {
                outputDesc = "—";
            }
            else
            {
                var baseOutput = GameBalance.BaseJobOutputPerSlot[job];
                var estimated = (int)Math.Floor(slots * multiplier * baseOutput);
                var outputResource = GameBalance.JobOutputResource[job];
                outputDesc = outputResource.HasValue
                    ? $"~{estimated} {outputResource.Value}"
                    : job == JobType.Repairs
                        ? $"~{estimated} integrity"
                        : $"~{estimated} care pts";
            }

            var shortcut = _noShortcuts ? "" : $"j{i + 1}: ";
            _out.WriteLine($"  {shortcut}{job,-18} {workers,3} workers  {slots} slot(s)  →  {outputDesc,-22} {JobInputShortDesc(job)}");
        }

        var idle = state.AvailableHealthyWorkersForAllocation - state.Allocation.TotalAssigned();
        _out.WriteLine($"  Idle: {(idle < 0 ? 0 : idle)} workers");
        _out.WriteLine();
    }

    private static string JobInputShortDesc(JobType job)
    {
        if (!GameBalance.JobInputPerSlot.TryGetValue(job, out var inputs) || inputs.Count == 0)
            return "";

        var parts = inputs.Select(kvp => $"{kvp.Value} {kvp.Key.ToString().ToLower()}");
        return $"consumes {string.Join(" + ", parts)}/slot";
    }

    private void RenderZones(GameState state)
    {
        _out.WriteLine("Zones");
        foreach (var zone in state.Zones)
        {
            var status = zone.IsLost ? "LOST" : "ACTIVE";
            var over = zone.Population - zone.Capacity;
            var overText = over > 0 ? $" (Over by {over})" : string.Empty;
            _out.WriteLine($"  {(int)zone.Id}. {zone.Name,-18} Integrity: {zone.Integrity,3}  Capacity: {zone.Capacity,3}  Housed: {zone.Population,3}  {status}{overText}");
        }

        _out.WriteLine();
    }

    private void RenderMissions(GameState state)
    {
        _out.WriteLine("Active Missions");
        if (state.ActiveMissions.Count == 0)
        {
            _out.WriteLine("  None");
        }
        else
        {
            foreach (var mission in state.ActiveMissions)
            {
                _out.WriteLine($"  {mission.MissionName}: {mission.DaysRemaining} day(s) remaining, {mission.WorkerCost} workers committed");
            }
        }

        _out.WriteLine();
    }

    private void RenderLaws(GameState state)
    {
        _out.WriteLine("Enacted Laws");
        if (state.ActiveLawIds.Count == 0)
        {
            _out.WriteLine("  None");
        }
        else
        {
            foreach (var lawId in state.ActiveLawIds)
            {
                var law = LawCatalog.Find(lawId);
                _out.WriteLine($"  {law?.Name ?? lawId}");
            }
        }

        _out.WriteLine();
    }

    private void RenderEvents()
    {
        _out.WriteLine("Possible Events");
        foreach (var evt in EventCatalog.GetAll())
        {
            _out.WriteLine($"  {evt.Name}: {evt.Description}");
        }

        _out.WriteLine();
    }

    private void RenderAvailableJobs()
    {
        _out.WriteLine("Assignable Job Slots");
        var jobs = ActionAvailability.GetJobTypes();
        for (var index = 0; index < jobs.Count; index++)
        {
            var job = jobs[index];
            var shortcut = _noShortcuts ? "" : $"j{index + 1}: ";
            _out.WriteLine($"  {shortcut}{job} | {JobDescription(job)}");
        }

        _out.WriteLine();
    }

    private void RenderAvailableLaws(GameState state)
    {
        _out.WriteLine("Available Laws");
        var available = ActionAvailability.GetAvailableLaws(state);
        if (available.Count == 0)
        {
            _out.WriteLine("  None currently available.");
            _out.WriteLine();
            return;
        }

        for (var index = 0; index < available.Count; index++)
        {
            var law = available[index];
            var shortcut = _noShortcuts ? "" : $"l{index + 1}: ";
            _out.WriteLine($"  {shortcut}{law.Name} ({law.Id}) | {law.Summary}");
        }

        _out.WriteLine();
    }

    private void RenderAvailableOrders(GameState state)
    {
        _out.WriteLine("Available Emergency Orders");
        var available = ActionAvailability.GetAvailableOrders(state);
        if (available.Count == 0)
        {
            _out.WriteLine("  None currently available.");
            _out.WriteLine();
            return;
        }

        for (var index = 0; index < available.Count; index++)
        {
            var order = available[index];
            var shortcut = _noShortcuts ? "" : $"o{index + 1}: ";
            if (!order.RequiresZoneSelection)
            {
                _out.WriteLine($"  {shortcut}{order.Name} ({order.Id}) | {order.Summary}");
                continue;
            }

            var validZones = ActionAvailability.GetValidZonesForOrder(state, order);
            var zoneList = validZones.Count == 0 ? "none" : string.Join(", ", validZones);
            _out.WriteLine($"  {shortcut}{order.Name} ({order.Id}) | {order.Summary} | valid ZoneId: {zoneList}");
        }

        _out.WriteLine();
    }

    private void RenderAvailableMissions(GameState state)
    {
        _out.WriteLine("Available Missions");
        var available = ActionAvailability.GetAvailableMissions(state);
        if (available.Count == 0)
        {
            _out.WriteLine("  None currently available.");
            _out.WriteLine();
            return;
        }

        for (var index = 0; index < available.Count; index++)
        {
            var mission = available[index];
            var shortcut = _noShortcuts ? "" : $"m{index + 1}: ";
            _out.WriteLine($"  {shortcut}{mission.Name} ({mission.Id}) | {mission.OutcomeHint}");
        }

        _out.WriteLine();
    }

    private static string JobDescription(JobType job)
    {
        var baseOutput = GameBalance.BaseJobOutputPerSlot[job];
        var outputResource = GameBalance.JobOutputResource[job];
        
        var outputDesc = outputResource.HasValue
            ? $"+{baseOutput} {outputResource.Value}"
            : job == JobType.Repairs
                ? $"+{baseOutput} integrity"
                : $"+{baseOutput} care pts";

        if (!GameBalance.JobInputPerSlot.TryGetValue(job, out var inputs) || inputs.Count == 0)
            return $"{job} ({outputDesc}).";

        var inputParts = inputs.Select(kvp => $"-{kvp.Value} {kvp.Key}");
        return $"{job} ({outputDesc}), {string.Join(", ", inputParts)}/slot).";
    }
}
