using System;
using System.IO;
using System.Linq;
using Prot8.Constants;
using Prot8.Events;
using Prot8.Jobs;
using Prot8.Cli.ViewModels;

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

    public void RenderDayStart(DayStartViewModel vm)
    {
        _out.WriteLine();
        _out.WriteLine($"Day {vm.Day} / {vm.TargetSurvivalDay}  |  Siege Intensity: {vm.SiegeIntensity}  |  Active Perimeter: {vm.ActivePerimeterName}");
        _out.WriteLine();

        RenderResources(vm);
        RenderPopulation(vm);
        RenderJobAssignments(vm);
        RenderZones(vm);
        RenderMissions(vm);
        RenderLaws(vm);
        RenderEvents();
        RenderActionReference(vm);
    }

    public void RenderPendingPlan(PendingPlanViewModel vm)
    {
        _out.WriteLine("Pending Day Plan");
        foreach (var job in vm.JobAssignments)
        {
            _out.WriteLine($"  {job.Job}: {job.Workers} workers");
        }

        _out.WriteLine($"  Total assigned: {vm.TotalAssigned} / {vm.AvailableWorkers}");
        _out.WriteLine($"  Idle workers: {vm.IdleWorkers}");

        if (vm.QueuedActionType is null)
        {
            _out.WriteLine("  Queued optional action: none");
        }
        else
        {
            var zoneSuffix = vm.QueuedActionZone is not null ? $" ({vm.QueuedActionZone})" : string.Empty;
            _out.WriteLine($"  Queued optional action: {vm.QueuedActionType} -> {vm.QueuedActionName}{zoneSuffix}");
        }

        _out.WriteLine();
    }

    public void RenderActionReference(DayStartViewModel vm)
    {
        RenderAvailableJobs(vm);
        RenderAvailableLaws(vm);
        RenderAvailableOrders(vm);
        RenderAvailableMissions(vm);
        
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

    public void RenderDayReport(DayReportViewModel vm)
    {
        _out.WriteLine();
        _out.WriteLine($"Day {vm.Day} Resolution");
        _out.WriteLine(new string('-', 76));

        foreach (var entry in vm.Entries)
        {
            _out.WriteLine($"[{entry.Tag}] {entry.Message}");
        }

        if (vm.TriggeredEvents.Count > 0)
        {
            _out.WriteLine();
            _out.WriteLine("Triggered Events");
            foreach (var evt in vm.TriggeredEvents)
            {
                _out.WriteLine($"  - {evt}");
            }
        }

        if (vm.ResolvedMissions.Count > 0)
        {
            _out.WriteLine();
            _out.WriteLine("Mission Outcomes");
            foreach (var mission in vm.ResolvedMissions)
            {
                _out.WriteLine($"  - {mission}");
            }
        }

        _out.WriteLine();
        _out.WriteLine("Recovery Status");
        _out.WriteLine($"  Recovery enabled: {(vm.RecoveryEnabledToday ? "Yes" : "No")}");
        _out.WriteLine($"  Recovered today: {vm.RecoveredWorkersToday}");
        _out.WriteLine($"  Medicine spent on recovery: {vm.RecoveryMedicineSpentToday}");
        if (!string.IsNullOrWhiteSpace(vm.RecoveryBlockedReason))
        {
            _out.WriteLine($"  Blocked reason: {vm.RecoveryBlockedReason}");
        }

        _out.WriteLine();
    }

    public void RenderFinal(GameOverViewModel vm)
    {
        _out.WriteLine();
        _out.WriteLine(new string('=', 76));
        if (vm.Survived)
        {
            _out.WriteLine($"You endured to Day {GameBalance.TargetSurvivalDay}. The city survives, but at great cost.");
        }
        else
        {
            _out.WriteLine($"Game Over on Day {vm.Day}: {vm.Cause}");
            if (!string.IsNullOrWhiteSpace(vm.Details))
            {
                _out.WriteLine(vm.Details);
            }
        }

        _out.WriteLine($"Total deaths: {vm.TotalDeaths}, total desertions: {vm.TotalDesertions}");
        _out.WriteLine($"Final Morale, Unrest, Sickness shown on next screen.");
        _out.WriteLine(new string('=', 76));
        _out.WriteLine();
    }

    private void RenderResources(DayStartViewModel vm)
    {
        var res = vm.Resources;
        var pop = vm.Population.TotalPopulation;
        var foodNeed = (int)Math.Ceiling(pop * GameBalance.FoodPerPersonPerDay);
        var waterNeed = (int)Math.Ceiling(pop * GameBalance.WaterPerPersonPerDay);
        var fuelNeed = (int)Math.Ceiling(pop * GameBalance.FuelPerPersonPerDay);

        _out.WriteLine("Resources");
        _out.WriteLine($"  Food: {res.Food,4}  Water: {res.Water,4}  Fuel: {res.Fuel,4}  Medicine: {res.Medicine,4}  Materials: {res.Materials,4}");
        _out.WriteLine($"  Daily need ({pop} pop):  Food ~{foodNeed}  Water ~{waterNeed}  Fuel ~{fuelNeed}  [shortfall each day → +Unrest  −Morale  +Sickness]");
        _out.WriteLine($"  Morale: {vm.Morale,3}/100  Unrest: {vm.Unrest,3}/100  Sickness: {vm.Sickness,3}/100  {SicknessStatusNote(vm.Sickness)}");
        _out.WriteLine();
    }

    private static string SicknessStatusNote(int sickness)
    {
        if (sickness > 70)
            return "[recovery LOCKED | deaths each day]";
        if (sickness >= 40)
            return "[recovery LOCKED at ≥40]";
        return "[recovery enabled]";
    }

    private void RenderPopulation(DayStartViewModel vm)
    {
        var pop = vm.Population;
        _out.WriteLine("Population");
        _out.WriteLine($"  Healthy Workers: {pop.HealthyWorkers}");
        _out.WriteLine($"  Guards:          {pop.Guards}");
        _out.WriteLine($"  Sick Workers:    {pop.SickWorkers}");
        _out.WriteLine($"  Elderly:         {pop.Elderly}");
        _out.WriteLine($"  Total:           {pop.TotalPopulation}");
        _out.WriteLine($"  Workers reserved on missions: {vm.ActiveMissions.Sum(m => m.WorkerCost)}");
        _out.WriteLine($"  Available for assignment today: {vm.Population.HealthyWorkers - vm.ActiveMissions.Sum(m => m.WorkerCost)}");
        _out.WriteLine();
    }

    private void RenderJobAssignments(DayStartViewModel vm)
    {
        var jobs = ActionAvailability.GetJobTypes();
        _out.WriteLine("Job Assignments");

        for (var i = 0; i < jobs.Count; i++)
        {
            var job = jobs[i];
            var assignment = vm.JobAssignments.FirstOrDefault(a => a.Job == job);
            var workers = assignment?.Workers ?? 0;
            var slots = workers / JobAllocation.Step;

            string outputDesc;
            if (slots == 0)
            {
                outputDesc = "—";
            }
            else
            {
                var baseOutput = GameBalance.BaseJobOutputPerSlot[job];
                var outputResource = GameBalance.JobOutputResource[job];
                outputDesc = outputResource.HasValue
                    ? $"+{baseOutput} {outputResource.Value}"
                    : job == JobType.Repairs
                        ? $"+{baseOutput} integrity"
                        : $"+{baseOutput} care pts";
            }

            var shortcut = _noShortcuts ? "" : $"j{i + 1}: ";
            _out.WriteLine($"  {shortcut}{job,-18} {workers,3} workers  {slots} slot(s)  →  {outputDesc,-22}");
        }

        _out.WriteLine();
    }

    private void RenderZones(DayStartViewModel vm)
    {
        _out.WriteLine("Zones");
        foreach (var zone in vm.Zones)
        {
            var status = zone.IsLost ? "LOST" : "ACTIVE";
            var over = zone.Population - zone.Capacity;
            var overText = over > 0 ? $" (Over by {over})" : string.Empty;
            _out.WriteLine($"  {(int)zone.Id}. {zone.Name,-18} Integrity: {zone.Integrity,3}  Capacity: {zone.Capacity,3}  Housed: {zone.Population,3}  {status}{overText}");
        }

        _out.WriteLine();
    }

    private void RenderMissions(DayStartViewModel vm)
    {
        _out.WriteLine("Active Missions");
        if (vm.ActiveMissions.Count == 0)
        {
            _out.WriteLine("  None");
        }
        else
        {
            foreach (var mission in vm.ActiveMissions)
            {
                _out.WriteLine($"  {mission.MissionName}: {mission.DaysRemaining} day(s) remaining, {mission.WorkerCost} workers committed");
            }
        }

        _out.WriteLine();
    }

    private void RenderLaws(DayStartViewModel vm)
    {
        _out.WriteLine("Enacted Laws");
        if (vm.AvailableLaws.Count == 0 || !vm.AvailableLaws.Any(l => l.IsActive))
        {
            _out.WriteLine("  None");
        }
        else
        {
            foreach (var law in vm.AvailableLaws.Where(l => l.IsActive))
            {
                _out.WriteLine($"  {law.Name}");
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

    private void RenderAvailableJobs(DayStartViewModel vm)
    {
        _out.WriteLine("Assignable Job Slots");
        for (var index = 0; index < vm.Jobs.Count; index++)
        {
            var job = vm.Jobs[index];
            _out.WriteLine($"  {job.Shortcut}{job.Job} | {job.Description}");
        }

        _out.WriteLine();
    }

    private void RenderAvailableLaws(DayStartViewModel vm)
    {
        _out.WriteLine("Available Laws");
        var available = vm.AvailableLaws.Where(l => !l.IsActive).ToList();
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
            _out.WriteLine($"  {shortcut}{law.Name} ({law.Id}) | {law.Tooltip}");
        }

        _out.WriteLine();
    }

    private void RenderAvailableOrders(DayStartViewModel vm)
    {
        _out.WriteLine("Available Emergency Orders");
        var available = vm.AvailableOrders;
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
                _out.WriteLine($"  {shortcut}{order.Name} ({order.Id}) | {order.Tooltip}");
                continue;
            }

            var zoneList = order.ValidZones.Count == 0 ? "none" : string.Join(", ", order.ValidZones);
            _out.WriteLine($"  {shortcut}{order.Name} ({order.Id}) | {order.Tooltip} | valid ZoneId: {zoneList}");
        }

        _out.WriteLine();
    }

    private void RenderAvailableMissions(DayStartViewModel vm)
    {
        _out.WriteLine("Available Missions");
        var available = vm.AvailableMissions;
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
            _out.WriteLine($"  {shortcut}{mission.Name} ({mission.Id}) | {mission.Tooltip}");
        }

        _out.WriteLine();
    }
}
