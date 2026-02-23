using Prot8.Cli.ViewModels;
using Prot8.Constants;

namespace Prot8.Cli.Output;

public sealed class ConsoleRenderer(TextWriter output)
{
    public void RenderDayStart(DayStartViewModel vm)
    {
        output.WriteLine();
        output.WriteLine($"=== DAY {vm.Day}/{vm.TargetSurvivalDay}  Siege:{vm.SiegeIntensity}  Perimeter:{vm.ActivePerimeterName} ===");
        output.WriteLine();

        if (vm.MoodLine is not null)
        {
            output.WriteLine($"  \"{vm.MoodLine}\"");
            output.WriteLine();
        }

        if (vm.DisruptionText is not null)
        {
            output.WriteLine($"*** {vm.DisruptionText} ***");
            output.WriteLine();
        }

        RenderResources(vm);
        RenderPopulation(vm);

        if (vm.ThreatProjection is not null)
        {
            output.WriteLine(vm.ThreatProjection);
            output.WriteLine();
        }

        if (vm.ProductionForecast is not null)
        {
            output.WriteLine(vm.ProductionForecast);
            output.WriteLine();
        }

        RenderJobs(vm);
        RenderZones(vm);

        if (vm.ZoneWarnings is not null)
        {
            output.WriteLine(vm.ZoneWarnings);
            output.WriteLine();
        }

        RenderMissions(vm);
        RenderLaws(vm);
        RenderActionReference(vm);
    }

    public void RenderPendingDayAction(PendingPlanViewModel vm)
    {
        output.WriteLine($"Action: {(vm.QueuedActionType is null ? "none" : $"{vm.QueuedActionType} -> {vm.QueuedActionName}")}");
        if (vm.QueuedDecreeType is not null)
        {
            output.WriteLine($"Decree: {vm.QueuedDecreeType} -> {vm.QueuedDecreeName}");
        }
    }

    public void RenderActionReference(DayStartViewModel vm)
    {
        RenderAvailableLaws(vm);
        RenderAvailableOrders(vm);
        RenderAvailableMissions(vm);
        RenderAvailableDecrees(vm);

        output.WriteLine("Commands (<> required): assign <Job> <N>  |  enact <LawId>  |  order <OrderId>  |  mission <MissionId>  |  decree <DecreeId>  |  clear_assignments  |  clear_action  |  end_day  |  help");
        output.WriteLine();
    }

    public void RenderDayReport(DayReportViewModel vm)
    {
        output.WriteLine();
        output.WriteLine($"--- Day {vm.Day} Resolution ---");

        if (vm.DeltaSummary is not null)
        {
            output.WriteLine(vm.DeltaSummary);
        }

        output.WriteLine();

        foreach (var entry in vm.Entries)
            output.WriteLine($"[{entry.Tag}] {entry.Message}");

        if (vm.TriggeredEvents.Count > 0)
            output.WriteLine($"Events: {string.Join(", ", vm.TriggeredEvents)}");

        if (vm.ResolvedMissions.Count > 0)
            output.WriteLine($"Missions: {string.Join(", ", vm.ResolvedMissions)}");

        var recoveryStatus = vm.RecoveryEnabledToday
            ? $"recovered {vm.RecoveredWorkersToday} workers (medicine -{vm.RecoveryMedicineSpentToday})"
            : $"blocked — {vm.RecoveryBlockedReason}";
        output.WriteLine($"Recovery: {recoveryStatus}");

        if (vm.AllocationAlert is not null)
        {
            output.WriteLine();
            output.WriteLine(vm.AllocationAlert);
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
        var foodNeed = (int)Math.Ceiling(pop * GameBalance.FoodPerPersonPerDay * vm.FoodConsumptionMultiplier);
        var waterNeed = (int)Math.Ceiling(pop * GameBalance.WaterPerPersonPerDay * vm.WaterConsumptionMultiplier);
        var fuelNeed = (int)Math.Ceiling(pop * GameBalance.FuelPerPersonPerDay);

        output.WriteLine($"Resources  Food:{res.Food,4}  Water:{res.Water,4}  Fuel:{res.Fuel,4}  Medicine:{res.Medicine,4}  Materials:{res.Materials,4}");
        output.WriteLine($"           Need/day ({pop} pop): Food~{foodNeed} Water~{waterNeed} Fuel~{fuelNeed}  |  Morale:{vm.Morale,3}  Unrest:{vm.Unrest,3}  Sickness:{vm.Sickness,3}  {SicknessStatusNote(vm.Sickness)}");
        output.WriteLine();
    }

    static string SicknessStatusNote(int sickness)
    {
        if (sickness > 70)
        {
            return "[recovery LOCKED | deaths each day]";
        }

        if (sickness >= 50)
        {
            return "[recovery LOCKED at ≥50]";
        }

        return "[recovery enabled]";
    }

    void RenderPopulation(DayStartViewModel vm)
    {
        var pop = vm.Population;
        var onMissions = vm.ActiveMissions.Sum(m => m.WorkerCost);
        output.WriteLine($"Population  Healthy:{pop.HealthyWorkers}  Guards:{pop.Guards}  Sick:{pop.SickWorkers}  Elderly:{pop.Elderly}  (Total:{pop.TotalPopulation})  |  On missions:{onMissions}  |  Available:{pop.HealthyWorkers - onMissions}");
        output.WriteLine();
    }

    void RenderJobs(DayStartViewModel vm)
    {
        output.WriteLine("Jobs");
        foreach (var (jobType, jvm) in vm.Jobs)
        {
            var inputs = string.Join(", ", jvm.CurrentInput.Select(x => x.ToString()));
            var outputs = string.Join(", ", jvm.CurrentOutput.Select(x => x.ToString()));
            var perWorker = string.Join(", ", jvm.OutputPerWorker.Select(x => x.ToString()));
            var inputStr = inputs.Length > 0 ? $"{inputs} -> " : "";
            output.WriteLine($"  {jobType,-18}  {jvm.AssignedWorkers,3} wkrs  |  {inputStr}{outputs}  (+{perWorker}/wkr)");
        }
        output.WriteLine();
    }

    void RenderZones(DayStartViewModel vm)
    {
        output.WriteLine("Zones");
        foreach (var zone in vm.Zones)
        {
            var status = zone.IsLost ? "LOST  " : "active";
            var over = zone.Population - zone.Capacity;
            var overText = over > 0 ? $" OVER+{over}" : string.Empty;
            output.WriteLine($"  {(int)zone.Id}. {zone.Name,-18}  [{status}]  Int:{zone.Integrity,3}  Cap:{zone.Capacity,3}  Pop:{zone.Population,3}{overText}");
        }
        output.WriteLine();
    }

    void RenderMissions(DayStartViewModel vm)
    {
        if (vm.ActiveMissions.Count == 0)
        {
            output.WriteLine("Active Missions  None");
        }
        else
        {
            var list = string.Join("  |  ", vm.ActiveMissions.Select(m => $"{m.MissionName}: {m.DaysRemaining}d, {m.WorkerCost} wkrs"));
            output.WriteLine($"Active Missions  {list}");
        }
        output.WriteLine();
    }

    void RenderLaws(DayStartViewModel vm)
    {
        var active = vm.AvailableLaws.Where(l => l.IsActive).Select(l => l.Name).ToList();
        output.WriteLine($"Enacted Laws  {(active.Count == 0 ? "None" : string.Join(", ", active))}");
        output.WriteLine();
    }

    void RenderAvailableLaws(DayStartViewModel vm)
    {
        var available = vm.AvailableLaws.Where(l => !l.IsActive).ToList();
        if (available.Count == 0)
        {
            output.WriteLine("Available Laws  None");
            return;
        }
        output.WriteLine("Available Laws");
        foreach (var law in available)
            output.WriteLine($"  {law.Id,-20} {law.Name} | {law.Tooltip}");
        output.WriteLine();
    }

    void RenderAvailableOrders(DayStartViewModel vm)
    {
        if (vm.OrderCooldownDaysRemaining > 0)
        {
            output.WriteLine($"Available Orders  On cooldown ({vm.OrderCooldownDaysRemaining}d remaining)");
            return;
        }
        if (vm.AvailableOrders.Count == 0)
        {
            output.WriteLine("Available Orders  None");
            return;
        }
        output.WriteLine("Available Orders");
        foreach (var order in vm.AvailableOrders)
            output.WriteLine($"  {order.Id,-20} {order.Name} | {order.Tooltip}");
        output.WriteLine();
    }

    void RenderAvailableDecrees(DayStartViewModel vm)
    {
        if (vm.AvailableDecrees.Count == 0)
        {
            output.WriteLine("Available Decrees  None");
            return;
        }
        output.WriteLine("Available Decrees (1 per day, no cooldown, in addition to law/order/mission)");
        foreach (var decree in vm.AvailableDecrees)
            output.WriteLine($"  {decree.Id,-20} {decree.Name} | {decree.Tooltip}");
        output.WriteLine();
    }

    void RenderAvailableMissions(DayStartViewModel vm)
    {
        if (vm.AvailableMissions.Count == 0)
        {
            output.WriteLine("Available Missions  None");
            return;
        }
        output.WriteLine("Available Missions");
        foreach (var mission in vm.AvailableMissions)
            output.WriteLine($"  {mission.Id,-20} {mission.Name} ({mission.DurationDays}d, {mission.RequiredIdleWorkers} wkrs) | {mission.Tooltip}");
        output.WriteLine();
    }
}