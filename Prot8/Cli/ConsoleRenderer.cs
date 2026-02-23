using Prot8.Cli.ViewModels;
using Prot8.Constants;
using Prot8.Simulation;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Prot8.Cli.Output;

public sealed class ConsoleRenderer(IAnsiConsole console)
{
    static string Esc(string text) => Markup.Escape(text);

    public void Clear() => console.Clear(false);

    public void RenderDayStart(DayStartViewModel vm, ActionTab activeTab = ActionTab.Laws)
    {
        console.WriteLine();
        console.Write(new Rule($"[bold yellow]DAY {vm.Day}/{vm.TargetSurvivalDay}  Siege:{vm.SiegeIntensity}  Perimeter:{Esc(vm.ActivePerimeterName)}[/]")
        {
            Style = Style.Parse("yellow")
        });
        console.WriteLine();

        if (vm.MoodLine is not null)
        {
            console.MarkupLine($"  [italic]\"{Esc(vm.MoodLine)}\"[/]");
            console.WriteLine();
        }

        if (vm.DisruptionText is not null)
        {
            console.MarkupLine($"[bold red]*** {Esc(vm.DisruptionText)} ***[/]");
            console.WriteLine();
        }

        RenderResources(vm);
        RenderPopulation(vm);

        if (vm.GlobalProductionMultiplier < 1.0)
            console.MarkupLine($"  [yellow]Production multiplier: {vm.GlobalProductionMultiplier:F2}x[/]");
        else if (vm.GlobalProductionMultiplier > 1.0)
            console.MarkupLine($"  [green]Production multiplier: {vm.GlobalProductionMultiplier:F2}x[/]");

        if (vm.ConsecutiveFoodDeficitDays > 0)
            console.MarkupLine($"  [red]Food deficit: {vm.ConsecutiveFoodDeficitDays} consecutive day(s)[/]");
        if (vm.ConsecutiveWaterDeficitDays > 0)
            console.MarkupLine($"  [red]Water deficit: {vm.ConsecutiveWaterDeficitDays} consecutive day(s)[/]");
        if (vm.ConsecutiveBothZeroDays > 0)
            console.MarkupLine($"  [bold red]Both food & water zero: {vm.ConsecutiveBothZeroDays} day(s)[/]");
        if (vm.OvercrowdingStacks > 0)
            console.MarkupLine($"  [yellow]Overcrowding: {vm.OvercrowdingStacks} stack(s) (+{vm.OvercrowdingStacks * 3} unrest/sickness per day)[/]");
        if (vm.SiegeEscalationDelayDays > 0)
            console.MarkupLine($"  [cyan]Siege escalation delayed: {vm.SiegeEscalationDelayDays} day(s)[/]");

        console.WriteLine();

        if (vm.ThreatProjection is not null)
        {
            console.MarkupLine(Esc(vm.ThreatProjection));
            console.WriteLine();
        }

        if (vm.ProductionForecast is not null)
        {
            console.MarkupLine(Esc(vm.ProductionForecast));
            console.WriteLine();
        }

        RenderJobs(vm);
        RenderZones(vm);

        if (vm.ZoneWarnings is not null)
        {
            console.MarkupLine($"[bold yellow]{Esc(vm.ZoneWarnings)}[/]");
            console.WriteLine();
        }

        RenderMissions(vm);
        RenderLaws(vm);

        RenderTabBar(vm, activeTab);
        RenderSelectedTab(vm, activeTab);
        RenderCommandPanel();
    }

    public void RenderPendingDayAction(PendingPlanViewModel vm)
    {
        if (vm.QueuedActionType is null)
        {
            console.MarkupLine("Action: [grey]none[/]");
        }
        else
        {
            console.MarkupLine($"Action: [cyan]{Esc(vm.QueuedActionType)}[/] -> [bold]{Esc(vm.QueuedActionName ?? "")}[/]");
        }

        if (vm.QueuedDecreeType is not null)
        {
            console.MarkupLine($"Decree: [magenta]{Esc(vm.QueuedDecreeType)}[/] -> [bold]{Esc(vm.QueuedDecreeName ?? "")}[/]");
        }
    }

    public void RenderActionReference(DayStartViewModel vm)
    {
        RenderAvailableLaws(vm);
        RenderAvailableOrders(vm);
        RenderAvailableMissions(vm);
        RenderAvailableDecrees(vm);
        RenderCommandPanel();
    }

    void RenderTabBar(DayStartViewModel vm, ActionTab activeTab)
    {
        var lawCount = vm.AvailableLaws.Count(l => !l.IsActive);
        var orderCount = vm.AvailableOrders.Count;
        var missionCount = vm.AvailableMissions.Count;
        var decreeCount = vm.AvailableDecrees.Count;

        var lawCd = vm.LawCooldownDaysRemaining > 0 ? $" cd:{vm.LawCooldownDaysRemaining}d" : "";
        var orderCd = vm.OrderCooldownDaysRemaining > 0 ? $" cd:{vm.OrderCooldownDaysRemaining}d" : "";

        string TabLabel(string name, int count, string extra, ActionTab tab)
        {
            var label = $"{name} ({count}){extra}";
            return tab == activeTab ? $"[bold underline white on blue] {Esc(label)} [/]" : $" {Esc(label)} ";
        }

        var bar = TabLabel("Laws", lawCount, lawCd, ActionTab.Laws)
                + " " + TabLabel("Orders", orderCount, orderCd, ActionTab.Orders)
                + " " + TabLabel("Missions", missionCount, "", ActionTab.Missions)
                + " " + TabLabel("Decrees", decreeCount, "", ActionTab.Decrees);

        console.MarkupLine(bar);
        console.WriteLine();
    }

    void RenderSelectedTab(DayStartViewModel vm, ActionTab activeTab)
    {
        switch (activeTab)
        {
            case ActionTab.Laws:
                RenderAvailableLaws(vm);
                break;
            case ActionTab.Orders:
                RenderAvailableOrders(vm);
                break;
            case ActionTab.Missions:
                RenderAvailableMissions(vm);
                break;
            case ActionTab.Decrees:
                RenderAvailableDecrees(vm);
                break;
        }
    }

    void RenderCommandPanel()
    {
        var panel = new Panel(
            new Markup(
                "[bold]assign[/] [grey]<Job> <N>[/]  [grey]|[/]  " +
                "[bold]enact[/] [grey]<LawId>[/]  [grey]|[/]  " +
                "[bold]order[/] [grey]<OrderId>[/]  [grey]|[/]  " +
                "[bold]mission[/] [grey]<MissionId>[/]  [grey]|[/]  " +
                "[bold]decree[/] [grey]<DecreeId>[/]  [grey]|[/]  " +
                "[bold]clear_assignments[/]  [grey]|[/]  " +
                "[bold]clear_action[/]  [grey]|[/]  " +
                "[bold]end_day[/]  [grey]|[/]  " +
                "[bold]view[/] [grey]<tab>[/]  [grey]|[/]  " +
                "[bold]help[/]"))
        {
            Header = new PanelHeader("Commands (<> = required)"),
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 0)
        };
        console.Write(panel);
        console.WriteLine();
    }

    public void RenderDayReport(DayReportViewModel vm)
    {
        console.WriteLine();
        console.Write(new Rule($"[bold]Day {vm.Day} Resolution[/]") { Style = Style.Parse("blue") });

        if (vm.DeltaSummary is not null)
        {
            console.MarkupLine(Esc(vm.DeltaSummary));
        }

        console.WriteLine();

        foreach (var entry in vm.Entries)
        {
            var color = GetTagColor(entry.Tag);
            console.MarkupLine($"[{color}][[{Esc(entry.Tag)}]][/] {Esc(entry.Message)}");
        }

        if (vm.TriggeredEvents.Count > 0)
        {
            console.MarkupLine($"[bold orange1]Events:[/] {Esc(string.Join(", ", vm.TriggeredEvents))}");
        }

        if (vm.ResolvedMissions.Count > 0)
        {
            console.MarkupLine($"[bold dodgerblue1]Missions:[/] {Esc(string.Join(", ", vm.ResolvedMissions))}");
        }

        if (vm.RecoveryEnabledToday)
        {
            console.MarkupLine($"[green]Recovery:[/] recovered {vm.RecoveredWorkersToday} workers (medicine -{vm.RecoveryMedicineSpentToday})");
        }
        else
        {
            console.MarkupLine($"[yellow]Recovery:[/] blocked — {Esc(vm.RecoveryBlockedReason ?? "unknown")}");
        }

        if (vm.AllocationAlert is not null)
        {
            console.WriteLine();
            console.MarkupLine($"[bold yellow]{Esc(vm.AllocationAlert)}[/]");
        }

        console.WriteLine();
    }

    public void RenderFinal(GameOverViewModel vm)
    {
        console.WriteLine();

        if (vm.Survived)
        {
            var content = new Rows(
                new Markup($"You endured to Day {GameBalance.TargetSurvivalDay}. The city survives, but at great cost."),
                new Markup($"Total deaths: [red]{vm.TotalDeaths}[/], total desertions: [yellow]{vm.TotalDesertions}[/]"),
                new Markup("Final Morale, Unrest, Sickness shown on next screen.")
            );
            var panel = new Panel(content)
            {
                Header = new PanelHeader("[bold green]VICTORY[/]"),
                Border = BoxBorder.Double,
                BorderStyle = Style.Parse("green"),
                Padding = new Padding(2, 1)
            };
            console.Write(panel);
        }
        else
        {
            var lines = new List<Markup>
            {
                new($"[bold]Game Over on Day {vm.Day}:[/] {Esc(vm.Cause.ToString())}")
            };
            if (!string.IsNullOrWhiteSpace(vm.Details))
            {
                lines.Add(new Markup(Esc(vm.Details)));
            }
            lines.Add(new Markup($"Total deaths: [red]{vm.TotalDeaths}[/], total desertions: [yellow]{vm.TotalDesertions}[/]"));
            lines.Add(new Markup("Final Morale, Unrest, Sickness shown on next screen."));

            var panel = new Panel(new Rows(lines.Cast<IRenderable>()))
            {
                Header = new PanelHeader("[bold red]DEFEAT[/]"),
                Border = BoxBorder.Double,
                BorderStyle = Style.Parse("red"),
                Padding = new Padding(2, 1)
            };
            console.Write(panel);
        }

        console.WriteLine();
    }

    void RenderResources(DayStartViewModel vm)
    {
        var res = vm.Resources;
        var pop = vm.Population.TotalPopulation;
        var foodNeed = (int)Math.Ceiling(pop * GameBalance.FoodPerPersonPerDay * vm.FoodConsumptionMultiplier);
        var waterNeed = (int)Math.Ceiling(pop * GameBalance.WaterPerPersonPerDay * vm.WaterConsumptionMultiplier);
        var fuelNeed = (int)Math.Ceiling(pop * GameBalance.FuelPerPersonPerDay);

        var table = new Table { Border = TableBorder.Rounded };
        table.AddColumn(new TableColumn("[bold]Food[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Water[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Fuel[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Medicine[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Materials[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Morale[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Unrest[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Sickness[/]").Centered());

        table.AddRow(
            res.Food.ToString(),
            res.Water.ToString(),
            res.Fuel.ToString(),
            res.Medicine.ToString(),
            res.Materials.ToString(),
            MoraleMarkup(vm.Morale),
            UnrestMarkup(vm.Unrest),
            SicknessMarkup(vm.Sickness) + " " + SicknessStatusNote(vm.Sickness));

        table.AddRow(
            $"[grey]~{foodNeed}/d[/]",
            $"[grey]~{waterNeed}/d[/]",
            $"[grey]~{fuelNeed}/d[/]",
            "[grey]-[/]",
            "[grey]-[/]",
            "[grey]-[/]",
            "[grey]-[/]",
            $"[grey]({pop} pop)[/]");

        table.Title = new TableTitle("[bold]Resources[/]");
        console.Write(table);
        console.WriteLine();
    }

    void RenderPopulation(DayStartViewModel vm)
    {
        var pop = vm.Population;
        var onMissions = vm.ActiveMissions.Sum(m => m.WorkerCost);
        var available = pop.HealthyWorkers - onMissions;

        var grid = new Grid();
        grid.AddColumn(); grid.AddColumn(); grid.AddColumn(); grid.AddColumn();
        grid.AddColumn(); grid.AddColumn(); grid.AddColumn();

        grid.AddRow(
            "[bold]Healthy[/]", "[bold]Guards[/]", "[bold]Sick[/]", "[bold]Elderly[/]",
            "[bold]Total[/]", "[bold]On missions[/]", "[bold]Available[/]");
        grid.AddRow(
            pop.HealthyWorkers.ToString(),
            pop.Guards.ToString(),
            pop.SickWorkers.ToString(),
            pop.Elderly.ToString(),
            pop.TotalPopulation.ToString(),
            onMissions.ToString(),
            $"[bold]{available}[/]");

        console.MarkupLine("[bold]Population[/]");
        console.Write(grid);

        if (vm.Population.SickWorkers > 0)
        {
            var recoveryInfo = vm.Population.RecoveryDaysAtCurrentSickness >= 999
                ? "[red]Recovery locked (sickness >= 50)[/]"
                : $"Recovery: ~{vm.Population.RecoveryDaysAtCurrentSickness}d at current sickness";
            var readyStr = vm.Population.SickReadyToRecover > 0
                ? $"  [green]{vm.Population.SickReadyToRecover} ready to recover[/]"
                : "";
            console.MarkupLine($"  {recoveryInfo}{readyStr}");
        }

        console.WriteLine();
    }

    void RenderJobs(DayStartViewModel vm)
    {
        var table = new Table { Border = TableBorder.Simple };
        table.Title = new TableTitle("[bold]Jobs[/]");
        table.AddColumn("Job");
        table.AddColumn(new TableColumn("Workers").RightAligned());
        table.AddColumn("Input → Output");
        table.AddColumn("+Per Worker");

        foreach (var (jobType, jvm) in vm.Jobs)
        {
            var inputs = string.Join(", ", jvm.CurrentInput.Select(x => x.ToString()));
            var outputs = string.Join(", ", jvm.CurrentOutput.Select(x => x.ToString()));
            var perWorker = string.Join(", ", jvm.OutputPerWorker.Select(x => x.ToString()));
            var inputStr = inputs.Length > 0 ? $"{Esc(inputs)} → " : "";
            table.AddRow(
                Esc(jobType.ToString()),
                jvm.AssignedWorkers.ToString(),
                $"{inputStr}{Esc(outputs)}",
                $"+{Esc(perWorker)}");
        }

        console.Write(table);
        console.WriteLine();
    }

    void RenderZones(DayStartViewModel vm)
    {
        var table = new Table { Border = TableBorder.Simple };
        table.Title = new TableTitle("[bold]Zones[/]");
        table.AddColumn("#");
        table.AddColumn("Name");
        table.AddColumn("Status");
        table.AddColumn(new TableColumn("Integrity").RightAligned());
        table.AddColumn(new TableColumn("Capacity").RightAligned());
        table.AddColumn(new TableColumn("Population").RightAligned());

        foreach (var zone in vm.Zones)
        {
            var status = zone.IsLost ? "[red]LOST[/]" : "[green]active[/]";
            var integrityColor = zone.Integrity switch
            {
                <= 25 => "bold red",
                <= 50 => "yellow",
                _ => "green"
            };
            var over = zone.Population - zone.Capacity;
            var overText = over > 0 ? $" [red]OVER+{over}[/]" : "";
            var nameMarkup = zone.IsLost ? $"[red]{Esc(zone.Name)}[/]" : Esc(zone.Name);

            table.AddRow(
                ((int)zone.Id).ToString(),
                nameMarkup,
                status,
                $"[{integrityColor}]{zone.Integrity}[/]",
                zone.Capacity.ToString(),
                $"{zone.Population}{overText}");
        }

        console.Write(table);
        console.WriteLine();
    }

    void RenderMissions(DayStartViewModel vm)
    {
        if (vm.ActiveMissions.Count == 0)
        {
            console.MarkupLine("[bold]Active Missions[/]  [grey]None[/]");
        }
        else
        {
            var list = string.Join("  [grey]|[/]  ",
                vm.ActiveMissions.Select(m => $"[dodgerblue1]{Esc(m.MissionName)}[/]: {m.DaysRemaining}d, {m.WorkerCost} wkrs"));
            console.MarkupLine($"[bold]Active Missions[/]  {list}");
        }

        if (vm.MissionCooldowns.Count > 0)
        {
            var cdList = string.Join(", ",
                vm.MissionCooldowns.Select(c => $"{Esc(c.MissionName)} ({c.DaysRemaining}d)"));
            console.MarkupLine($"  [yellow]Mission cooldowns: {cdList}[/]");
        }

        console.WriteLine();
    }

    void RenderLaws(DayStartViewModel vm)
    {
        var active = vm.AvailableLaws.Where(l => l.IsActive).Select(l => l.Name).ToList();
        if (active.Count == 0)
        {
            console.MarkupLine("[bold]Enacted Laws[/]  [grey]None[/]");
        }
        else
        {
            var list = string.Join(", ", active.Select(n => $"[green]{Esc(n)}[/]"));
            console.MarkupLine($"[bold]Enacted Laws[/]  {list}");
        }

        console.WriteLine();
    }

    void RenderAvailableLaws(DayStartViewModel vm)
    {
        if (vm.LawCooldownDaysRemaining > 0)
        {
            console.MarkupLine($"[bold]Available Laws[/]  [yellow]On cooldown ({vm.LawCooldownDaysRemaining}d remaining)[/]");
            return;
        }

        var available = vm.AvailableLaws.Where(l => !l.IsActive).ToList();
        if (available.Count == 0)
        {
            console.MarkupLine("[bold]Available Laws[/]  [grey]None[/]");
            return;
        }

        var table = new Table { Border = TableBorder.Simple };
        table.Title = new TableTitle("[bold]Available Laws[/]");
        table.AddColumn(new TableColumn("[green]ID[/]"));
        table.AddColumn("Name");
        table.AddColumn("Effect");

        foreach (var law in available)
            table.AddRow($"[green]{Esc(law.Id)}[/]", Esc(law.Name), Esc(law.Tooltip));

        console.Write(table);
        console.WriteLine();
    }

    void RenderAvailableOrders(DayStartViewModel vm)
    {
        if (vm.OrderCooldownDaysRemaining > 0)
        {
            console.MarkupLine($"[bold]Available Orders[/]  [yellow]On cooldown ({vm.OrderCooldownDaysRemaining}d remaining)[/]");
            return;
        }

        if (vm.AvailableOrders.Count == 0)
        {
            console.MarkupLine("[bold]Available Orders[/]  [grey]None[/]");
            return;
        }

        var table = new Table { Border = TableBorder.Simple };
        table.Title = new TableTitle("[bold]Available Orders[/]");
        table.AddColumn(new TableColumn("[orange1]ID[/]"));
        table.AddColumn("Name");
        table.AddColumn("Effect");

        foreach (var order in vm.AvailableOrders)
            table.AddRow($"[orange1]{Esc(order.Id)}[/]", Esc(order.Name), Esc(order.Tooltip));

        console.Write(table);
        console.WriteLine();
    }

    void RenderAvailableDecrees(DayStartViewModel vm)
    {
        if (vm.AvailableDecrees.Count == 0)
        {
            console.MarkupLine("[bold]Available Decrees[/]  [grey]None[/]");
            return;
        }

        var table = new Table { Border = TableBorder.Simple };
        table.Title = new TableTitle("[bold]Available Decrees[/] [grey](1 per day, no cooldown, in addition to law/order/mission)[/]");
        table.AddColumn(new TableColumn("[magenta]ID[/]"));
        table.AddColumn("Name");
        table.AddColumn("Effect");

        foreach (var decree in vm.AvailableDecrees)
            table.AddRow($"[magenta]{Esc(decree.Id)}[/]", Esc(decree.Name), Esc(decree.Tooltip));

        console.Write(table);
        console.WriteLine();
    }

    void RenderAvailableMissions(DayStartViewModel vm)
    {
        if (vm.AvailableMissions.Count == 0)
        {
            console.MarkupLine("[bold]Available Missions[/]  [grey]None[/]");
            return;
        }

        var table = new Table { Border = TableBorder.Simple };
        table.Title = new TableTitle("[bold]Available Missions[/]");
        table.AddColumn(new TableColumn("[dodgerblue1]ID[/]"));
        table.AddColumn("Name");
        table.AddColumn("Duration");
        table.AddColumn("Workers");
        table.AddColumn("Effect");

        foreach (var mission in vm.AvailableMissions)
            table.AddRow(
                $"[dodgerblue1]{Esc(mission.Id)}[/]",
                Esc(mission.Name),
                $"{mission.DurationDays}d",
                $"{mission.RequiredIdleWorkers} wkrs",
                Esc(mission.Tooltip));

        console.Write(table);
        console.WriteLine();
    }

    static string MoraleMarkup(int morale) => morale switch
    {
        < 25 => $"[bold red]{morale}[/]",
        < 50 => $"[yellow]{morale}[/]",
        _ => $"[green]{morale}[/]"
    };

    static string UnrestMarkup(int unrest) => unrest switch
    {
        > 70 => $"[bold red]{unrest}[/]",
        > 50 => $"[yellow]{unrest}[/]",
        _ => $"[green]{unrest}[/]"
    };

    static string SicknessMarkup(int sickness) => sickness switch
    {
        > 70 => $"[bold red]{sickness}[/]",
        >= 50 => $"[yellow]{sickness}[/]",
        _ => $"[green]{sickness}[/]"
    };

    static string SicknessStatusNote(int sickness) => sickness switch
    {
        > 70 => "[bold red][[recovery LOCKED | deaths each day]][/]",
        >= 50 => "[yellow][[recovery LOCKED at ≥50]][/]",
        _ => "[green][[recovery enabled]][/]"
    };

    static string GetTagColor(string tag) => tag switch
    {
        ReasonTags.LawPassive => "cyan",
        ReasonTags.LawEnact => "bold cyan",
        ReasonTags.OrderEffect => "bold orange1",
        ReasonTags.Production => "green",
        ReasonTags.Consumption => "grey",
        ReasonTags.Deficit => "bold red",
        ReasonTags.Overcrowding => "yellow",
        ReasonTags.Sickness => "red",
        ReasonTags.RecoveryProgress => "green",
        ReasonTags.RecoveryComplete => "bold green",
        ReasonTags.RecoveryBlockedThreshold => "yellow",
        ReasonTags.RecoveryBlockedMedicine => "yellow",
        ReasonTags.Unrest => "red",
        ReasonTags.Siege => "bold red",
        ReasonTags.Repairs => "blue",
        ReasonTags.Event => "bold orange1",
        ReasonTags.Mission => "dodgerblue1",
        ReasonTags.ZoneLoss => "bold red",
        _ => "white"
    };
}
