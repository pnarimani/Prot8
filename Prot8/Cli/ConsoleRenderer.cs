using Prot8.Cli.ViewModels;
using Prot8.Constants;
using Prot8.Events;

namespace Prot8.Cli.Output;

public sealed class ConsoleRenderer(TextWriter output)
{
    public void RenderDayStart(DayStartViewModel vm)
    {
        output.WriteLine();
        output.WriteLine(
            $"Day {vm.Day} / {vm.TargetSurvivalDay}  |  Siege Intensity: {vm.SiegeIntensity}  |  Active Perimeter: {vm.ActivePerimeterName}");
        output.WriteLine();

        RenderResources(vm);
        RenderPopulation(vm);
        RenderJobs(vm);
        RenderZones(vm);
        RenderMissions(vm);
        RenderLaws(vm);
        RenderEvents();
        RenderActionReference(vm);
    }

    public void RenderPendingDayAction(PendingPlanViewModel vm)
    {
        output.Write("Pending Day Action: ");
        output.Write(vm.QueuedActionType is null ? "none" : $"{vm.QueuedActionType} -> {vm.QueuedActionName}");
        output.WriteLine();
    }

    public void RenderActionReference(DayStartViewModel vm)
    {
        RenderAvailableLaws(vm);
        RenderAvailableOrders(vm);
        RenderAvailableMissions(vm);

        output.WriteLine();
        output.WriteLine("Available Commands (sections with < > are mandatory)");
        output.WriteLine(
            "  assign <JobType> <Workers>          Set workers for one production slot (absolute value, steps of 5).");
        output.WriteLine("  enact <LawId>                       Queue one available law for today.");
        output.WriteLine("  order <OrderId> [ZoneId]            Queue one available emergency order for today.");
        output.WriteLine("  mission <MissionId>                 Queue one available mission for today.");
        output.WriteLine("  clear_assignments                   Reset all job assignments to 0.");
        output.WriteLine("  clear_action                        Clear queued law/order/mission action.");
        output.WriteLine("  help                                Print this available-actions list.");
        output.WriteLine("  end_day                             Resolve simulation using current plan.");
        output.WriteLine();
    }

    public void RenderDayReport(DayReportViewModel vm)
    {
        output.WriteLine();
        output.WriteLine($"Day {vm.Day} Resolution");
        output.WriteLine(new string('-', 76));

        foreach (var entry in vm.Entries)
        {
            output.WriteLine($"[{entry.Tag}] {entry.Message}");
        }

        if (vm.TriggeredEvents.Count > 0)
        {
            output.WriteLine();
            output.WriteLine("Triggered Events");
            foreach (var evt in vm.TriggeredEvents)
            {
                output.WriteLine($"  - {evt}");
            }
        }

        if (vm.ResolvedMissions.Count > 0)
        {
            output.WriteLine();
            output.WriteLine("Mission Outcomes");
            foreach (var mission in vm.ResolvedMissions)
            {
                output.WriteLine($"  - {mission}");
            }
        }

        output.WriteLine();
        output.WriteLine("Recovery Status");
        output.WriteLine($"  Recovery enabled: {(vm.RecoveryEnabledToday ? "Yes" : "No")}");
        output.WriteLine($"  Recovered today: {vm.RecoveredWorkersToday}");
        output.WriteLine($"  Medicine spent on recovery: {vm.RecoveryMedicineSpentToday}");
        if (!string.IsNullOrWhiteSpace(vm.RecoveryBlockedReason))
        {
            output.WriteLine($"  Blocked reason: {vm.RecoveryBlockedReason}");
        }

        output.WriteLine();
    }

    public void RenderFinal(GameOverViewModel vm)
    {
        output.WriteLine();
        output.WriteLine(new string('=', 76));
        if (vm.Survived)
        {
            output.WriteLine(
                $"You endured to Day {GameBalance.TargetSurvivalDay}. The city survives, but at great cost.");
        }
        else
        {
            output.WriteLine($"Game Over on Day {vm.Day}: {vm.Cause}");
            if (!string.IsNullOrWhiteSpace(vm.Details))
            {
                output.WriteLine(vm.Details);
            }
        }

        output.WriteLine($"Total deaths: {vm.TotalDeaths}, total desertions: {vm.TotalDesertions}");
        output.WriteLine("Final Morale, Unrest, Sickness shown on next screen.");
        output.WriteLine(new string('=', 76));
        output.WriteLine();
    }

    void RenderResources(DayStartViewModel vm)
    {
        var res = vm.Resources;
        var pop = vm.Population.TotalPopulation;
        var foodNeed = (int)Math.Ceiling(pop * GameBalance.FoodPerPersonPerDay);
        var waterNeed = (int)Math.Ceiling(pop * GameBalance.WaterPerPersonPerDay);
        var fuelNeed = (int)Math.Ceiling(pop * GameBalance.FuelPerPersonPerDay);

        output.WriteLine("Resources");
        output.WriteLine(
            $"  Food: {res.Food,4}  Water: {res.Water,4}  Fuel: {res.Fuel,4}  Medicine: {res.Medicine,4}  Materials: {res.Materials,4}");
        output.WriteLine(
            $"  Daily need ({pop} pop):  Food ~{foodNeed}  Water ~{waterNeed}  Fuel ~{fuelNeed}  [shortfall each day → +Unrest  −Morale  +Sickness]");
        output.WriteLine(
            $"  Morale: {vm.Morale,3}/100  Unrest: {vm.Unrest,3}/100  Sickness: {vm.Sickness,3}/100  {SicknessStatusNote(vm.Sickness)}");
        output.WriteLine();
    }

    static string SicknessStatusNote(int sickness)
    {
        if (sickness > 70)
        {
            return "[recovery LOCKED | deaths each day]";
        }

        if (sickness >= 40)
        {
            return "[recovery LOCKED at ≥40]";
        }

        return "[recovery enabled]";
    }

    void RenderPopulation(DayStartViewModel vm)
    {
        var pop = vm.Population;
        output.WriteLine("Population");
        output.WriteLine($"  Healthy Workers: {pop.HealthyWorkers}");
        output.WriteLine($"  Guards:          {pop.Guards}");
        output.WriteLine($"  Sick Workers:    {pop.SickWorkers}");
        output.WriteLine($"  Elderly:         {pop.Elderly}");
        output.WriteLine($"  Total:           {pop.TotalPopulation}");
        output.WriteLine($"  Workers reserved on missions: {vm.ActiveMissions.Sum(m => m.WorkerCost)}");
        output.WriteLine(
            $"  Available for assignment today: {vm.Population.HealthyWorkers - vm.ActiveMissions.Sum(m => m.WorkerCost)}");
        output.WriteLine();
    }

    void RenderJobs(DayStartViewModel vm)
    {
        output.WriteLine("Jobs");

        foreach (var jvm in vm.Jobs)
        {
            var workers = jvm.AssignedWorkers;
            var inputsPerWorker = string.Join(", ", jvm.InputPerWorker.Select(x => x.ToString()));
            var outputsPerWorker = string.Join(", ", jvm.OutputPerWorker.Select(x => x.ToString()));
            var currentInputs = string.Join(", ", jvm.CurrentInput.Select(x => x.ToString()));
            var currentOutputs = string.Join(", ", jvm.CurrentOutput.Select(x => x.ToString()));
            output.WriteLine($" {jvm.Job,-18} | {workers,3} workers | {currentInputs,-12} -> {currentOutputs,-12} | {inputsPerWorker,-12} / per worker -> {outputsPerWorker,-12}");
        }

        output.WriteLine();
    }

    void RenderZones(DayStartViewModel vm)
    {
        output.WriteLine("Zones");
        foreach (var zone in vm.Zones)
        {
            var status = zone.IsLost ? "LOST" : "ACTIVE";
            var over = zone.Population - zone.Capacity;
            var overText = over > 0 ? $" (Over by {over})" : string.Empty;
            output.WriteLine(
                $"  {(int)zone.Id}. {zone.Name,-18} Integrity: {zone.Integrity,3}  Capacity: {zone.Capacity,3}  Housed: {zone.Population,3}  {status}{overText}");
        }

        output.WriteLine();
    }

    void RenderMissions(DayStartViewModel vm)
    {
        output.WriteLine("Active Missions");
        if (vm.ActiveMissions.Count == 0)
        {
            output.WriteLine("  None");
        }
        else
        {
            foreach (var mission in vm.ActiveMissions)
            {
                output.WriteLine(
                    $"  {mission.MissionName}: {mission.DaysRemaining} day(s) remaining, {mission.WorkerCost} workers committed");
            }
        }

        output.WriteLine();
    }

    void RenderLaws(DayStartViewModel vm)
    {
        output.WriteLine("Enacted Laws");
        if (vm.AvailableLaws.Count == 0 || !vm.AvailableLaws.Any(l => l.IsActive))
        {
            output.WriteLine("  None");
        }
        else
        {
            foreach (var law in vm.AvailableLaws.Where(l => l.IsActive))
            {
                output.WriteLine($"  {law.Name}");
            }
        }

        output.WriteLine();
    }

    void RenderEvents()
    {
        output.WriteLine("Possible Events");
        foreach (var evt in EventCatalog.GetAll())
        {
            output.WriteLine($"  {evt.Name}: {evt.Description}");
        }

        output.WriteLine();
    }

    void RenderAvailableLaws(DayStartViewModel vm)
    {
        output.WriteLine("Available Laws");
        var available = vm.AvailableLaws.Where(l => !l.IsActive).ToList();
        if (available.Count == 0)
        {
            output.WriteLine("  None currently available.");
            output.WriteLine();
            return;
        }

        foreach (var law in available)
        {
            output.WriteLine($"  {law.Name} ({law.Id}) | {law.Tooltip}");
        }

        output.WriteLine();
    }

    void RenderAvailableOrders(DayStartViewModel vm)
    {
        output.WriteLine("Available Emergency Orders");
        var available = vm.AvailableOrders;
        if (available.Count == 0)
        {
            output.WriteLine("  None currently available.");
            output.WriteLine();
            return;
        }

        foreach (var order in available)
        {
            output.WriteLine($"  {order.Name} ({order.Id}) | {order.Tooltip}");
        }

        output.WriteLine();
    }

    void RenderAvailableMissions(DayStartViewModel vm)
    {
        output.WriteLine("Available Missions");
        var available = vm.AvailableMissions;
        if (available.Count == 0)
        {
            output.WriteLine("  None currently available.");
            output.WriteLine();
            return;
        }

        foreach (var mission in available)
        {
            output.WriteLine($"  {mission.Name} ({mission.Id}) | {mission.Tooltip}");
        }

        output.WriteLine();
    }
}