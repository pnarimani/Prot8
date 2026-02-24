using Prot8.Cli.ViewModels;
using Prot8.Constants;
using Prot8.Events;
using Prot8.Simulation;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Prot8.Cli.Output;

public sealed class ConsoleRenderer(IAnsiConsole console)
{
    static string Esc(string text)
    {
        return Markup.Escape(text);
    }

    public void Clear()
    {
        console.Clear(false);
    }

    public void RenderDayStart(DayStartViewModel vm, ActionTab activeTab = ActionTab.Laws)
    {
        console.Write(
            new Rows(
                BuildHeader(vm),
                new Columns(BuildResourcesTable(vm), BuildPopulationTable(vm)),
                BuildStatusWarnings(vm),
                new Columns(BuildJobsTable(vm), BuildZonesTable(vm)),
                new Rule { Style = Style.Parse("grey") },
                BuildStateSection(vm),
                new Rule { Style = Style.Parse("grey") },
                BuildActionsSection(vm, activeTab),
                BuildCommandPanel()
            )
        );
    }

    static Rows BuildHeader(DayStartViewModel vm)
    {
        var items = new List<IRenderable>
        {
            new Rule(
                    $"[bold yellow]DAY {vm.Day}/{vm.TargetSurvivalDay}  Siege Intensity:{vm.SiegeIntensity}  Perimeter:{Esc(vm.ActivePerimeterName)}[/]")
                { Style = Style.Parse("yellow") },
        };

        if (vm.MoodLine is not null)
        {
            items.Add(Align.Center(new Markup($"[italic]\"{Esc(vm.MoodLine)}\"[/]")));
        }

        if (vm.SituationAlerts.Count > 0)
        {
            foreach (var alert in vm.SituationAlerts)
            {
                var color = alert.StartsWith("CRITICAL") ? "bold red" : "bold yellow";
                items.Add(Align.Center(new Markup($"[{color}]  {Esc(alert)}[/]")));
            }
        }

        if (vm.DisruptionText is not null)
        {
            items.Add(Align.Center(new Markup($"[bold red]*** {Esc(vm.DisruptionText)} ***[/]")));
        }

        return new Rows(items);
    }

    static IRenderable BuildStatusWarnings(DayStartViewModel vm)
    {
        var items = new List<IRenderable>();

        if (vm.GlobalProductionMultiplier < 1.0)
        {
            var reasons = vm.ProductionMultiplierReasons.Count > 0
                ? " ← " + string.Join(", ", vm.ProductionMultiplierReasons.Select(Esc))
                : "";
            items.Add(new Markup($"  [yellow]Production multiplier: {vm.GlobalProductionMultiplier:F2}x{reasons}[/]"));
        }
        else if (vm.GlobalProductionMultiplier > 1.0)
        {
            var reasons = vm.ProductionMultiplierReasons.Count > 0
                ? " ← " + string.Join(", ", vm.ProductionMultiplierReasons.Select(Esc))
                : "";
            items.Add(new Markup($"  [green]Production multiplier: {vm.GlobalProductionMultiplier:F2}x{reasons}[/]"));
        }

        if (vm.ConsecutiveFoodDeficitDays > 0)
        {
            items.Add(new Markup($"  [red]Food deficit: {vm.ConsecutiveFoodDeficitDays} consecutive day(s)[/]"));
        }

        if (vm.ConsecutiveWaterDeficitDays > 0)
        {
            items.Add(new Markup($"  [red]Water deficit: {vm.ConsecutiveWaterDeficitDays} consecutive day(s)[/]"));
        }

        if (vm.ConsecutiveBothZeroDays > 0)
        {
            items.Add(new Markup($"  [bold red]Both food & water zero: {vm.ConsecutiveBothZeroDays} day(s)[/]"));
        }

        if (vm.OvercrowdingStacks > 0)
        {
            items.Add(new Markup(
                $"  [yellow]Overcrowding: {vm.OvercrowdingStacks} stack(s) (+{vm.OvercrowdingStacks * 3} unrest/sickness per day)[/]"));
        }

        if (vm.SiegeEscalationDelayDays > 0)
        {
            items.Add(new Markup($"  [cyan]Siege escalation delayed: {vm.SiegeEscalationDelayDays} day(s)[/]"));
        }

        if (vm.ThreatProjection is not null)
        {
            items.Add(new Text(vm.ThreatProjection));
        }

        if (vm.ProductionForecast is not null)
        {
            items.Add(new Text(vm.ProductionForecast));
        }

        return items.Count > 0 ? new Rows(items) : new Text("");
    }

    static Table BuildResourcesTable(DayStartViewModel vm)
    {
        var res = vm.Resources;
        var pop = vm.Population.TotalPopulation;
        var foodNeed = (int)Math.Ceiling(pop * GameBalance.FoodPerPersonPerDay * vm.FoodConsumptionMultiplier);
        var waterNeed = (int)Math.Ceiling(pop * GameBalance.WaterPerPersonPerDay * vm.WaterConsumptionMultiplier);
        var fuelNeed = (int)Math.Ceiling(pop * GameBalance.FuelPerPersonPerDay);

        var table = new Table { Border = TableBorder.Rounded, Expand = true };
        table.AddColumn(new TableColumn("[bold]Food[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Water[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Fuel[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Medicine[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Materials[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Morale[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Unrest[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Sickness[/]").Centered());

        var moraleDeltaStr = FormatDelta(vm.MoraleDelta);
        var unrestDeltaStr = FormatDelta(vm.UnrestDelta);
        var sicknessDeltaStr = FormatDelta(vm.SicknessDelta);

        table.AddRow(
            res.Food.ToString(),
            res.Water.ToString(),
            res.Fuel.ToString(),
            res.Medicine.ToString(),
            res.Materials.ToString(),
            MoraleMarkup(vm.Morale) + $" [grey]({moraleDeltaStr})[/]",
            UnrestMarkup(vm.Unrest) + $" [grey]({unrestDeltaStr})[/]",
            SicknessMarkup(vm.Sickness) + " " + SicknessStatusNote(vm.Sickness) + $" [grey]({sicknessDeltaStr})[/]");

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
        return table;
    }

    static IRenderable BuildPopulationTable(DayStartViewModel vm)
    {
        var pop = vm.Population;
        var onMissions = vm.ActiveMissions.Sum(m => m.WorkerCost);
        var available = pop.HealthyWorkers - onMissions;

        var table = new Table { Border = TableBorder.Rounded, Expand = true };
        table.AddColumn(new TableColumn("[bold]Healthy[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Guards[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Sick[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Elderly[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Total[/]").Centered());
        table.AddColumn(new TableColumn("[bold]On missions[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Available[/]").Centered());

        table.AddRow(
            pop.HealthyWorkers.ToString(),
            pop.Guards.ToString(),
            pop.SickWorkers.ToString(),
            pop.Elderly.ToString(),
            pop.TotalPopulation.ToString(),
            onMissions.ToString(),
            $"[bold]{available}[/]");

        if (vm.Population.SickWorkers > 0)
        {
            var recoveryInfo = vm.Population.RecoveryDaysAtCurrentSickness >= 999
                ? "[red]No Recovery (sickness >= 50)[/]"
                : $"Recovery: ~{vm.Population.RecoveryDaysAtCurrentSickness}d";
            var readyStr = vm.Population.SickReadyToRecover > 0
                ? $"  [green]{vm.Population.SickReadyToRecover} to Recover[/]"
                : "";
            table.AddRow(
                new Markup($"{recoveryInfo}{readyStr}"),
                new Text(""), new Text(""), new Text(""),
                new Text(""), new Text(""), new Text(""));
        }

        table.Title = new TableTitle("[bold]Population[/]");
        return table;
    }

    static IRenderable BuildJobsTable(DayStartViewModel vm)
    {
        var table = new Table
        {
            Border = TableBorder.Horizontal,
            Expand = true,
            Title = new TableTitle("[bold]Jobs[/]"),
        };
        table.AddColumn("Job");
        table.AddColumn(new TableColumn("Workers").RightAligned());
        table.AddColumn("Current Input");
        table.AddColumn("Current Output");
        table.AddColumn("+Per Worker");

        foreach (var (jobType, jvm) in vm.Jobs)
        {
            var inputs = string.Join(", ", jvm.CurrentInput.Select(x => x.ToString()));
            var outputs = string.Join(", ", jvm.CurrentOutput.Select(x => x.ToString()));
            var perWorker = string.Join(", ", jvm.OutputPerWorker.Select(x => x.ToString()));
            var inputStr = inputs.Length > 0 ? $"{Esc(inputs)}" : " ";
            table.AddRow(
                Esc(jobType.ToString()),
                jvm.AssignedWorkers.ToString(),
                $"{inputStr}",
                Esc(outputs),
                $"+{Esc(perWorker)}");
        }

        return table;
    }

    static Table BuildZonesTable(DayStartViewModel vm)
    {
        var table = new Table
        {
            Border = TableBorder.Horizontal,
            Expand = true,
            Title = new TableTitle("[bold]Zones[/]"),
        };
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
                _ => "green",
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

        return table;
    }

    static IRenderable BuildStateSection(DayStartViewModel vm)
    {
        var items = new List<IRenderable>();

        // Active missions
        if (vm.ActiveMissions.Count == 0)
        {
            items.Add(new Markup("[bold]Active Missions[/]  [grey]None[/]"));
        }
        else
        {
            var list = string.Join("  [grey]|[/]  ",
                vm.ActiveMissions.Select(m =>
                    $"[dodgerblue1]{Esc(m.MissionName)}[/]: {m.DaysRemaining}d, {m.WorkerCost} wkrs"));
            items.Add(new Markup($"[bold]Active Missions[/]  {list}"));
        }

        // Mission cooldowns
        if (vm.MissionCooldowns.Count > 0)
        {
            var cdList = string.Join(", ",
                vm.MissionCooldowns.Select(c => $"{Esc(c.MissionName)} ({c.DaysRemaining}d)"));
            items.Add(new Markup($"  [yellow]Mission cooldowns: {cdList}[/]"));
        }

        // Enacted laws
        var active = vm.AvailableLaws.Where(l => l.IsActive).Select(l => l.Name).ToList();
        if (active.Count == 0)
        {
            items.Add(new Markup("[bold]Enacted Laws[/]  [grey]None[/]"));
        }
        else
        {
            var list = string.Join(", ", active.Select(n => $"[green]{Esc(n)}[/]"));
            items.Add(new Markup($"[bold]Enacted Laws[/]  {list}"));
        }

        // Zone warnings
        if (vm.ZoneWarnings is not null)
        {
            items.Add(new Markup($"[bold yellow]{Esc(vm.ZoneWarnings)}[/]"));
        }

        return new Rows(items);
    }

    static IRenderable BuildActionsSection(DayStartViewModel vm, ActionTab activeTab)
    {
        var items = new List<IRenderable> { BuildTabBar(vm, activeTab) };

        var tabContent = activeTab switch
        {
            ActionTab.Laws => BuildAvailableLaws(vm),
            ActionTab.Orders => BuildAvailableOrders(vm),
            ActionTab.Missions => BuildAvailableMissions(vm),
            _ => new Text(""),
        };

        items.Add(tabContent);
        return new Rows(items);
    }

    static IRenderable BuildTabBar(DayStartViewModel vm, ActionTab activeTab)
    {
        var lawCount = vm.AvailableLaws.Count(l => !l.IsActive);
        var orderCount = vm.AvailableOrders.Count;
        var missionCount = vm.AvailableMissions.Count;

        var lawCd = vm.LawCooldownDaysRemaining > 0 ? $" cd:{vm.LawCooldownDaysRemaining}d" : "";

        string TabLabel(string name, int count, string extra, ActionTab tab)
        {
            var label = $"{name} ({count}){extra}";
            return tab == activeTab ? $"[bold underline white on blue] {Esc(label)} [/]" : $" {Esc(label)} ";
        }

        var bar = TabLabel("Laws", lawCount, lawCd, ActionTab.Laws)
                  + " " + TabLabel("Orders", orderCount, "", ActionTab.Orders)
                  + " " + TabLabel("Missions", missionCount, "", ActionTab.Missions);

        return new Markup(bar);
    }

    static IRenderable BuildAvailableLaws(DayStartViewModel vm)
    {
        if (vm.LawCooldownDaysRemaining > 0)
        {
            return new Markup(
                $"[bold]Available Laws[/]  [yellow]On cooldown ({vm.LawCooldownDaysRemaining}d remaining)[/]");
        }

        var available = vm.AvailableLaws.Where(l => !l.IsActive).ToList();
        if (available.Count == 0)
        {
            return new Markup("[bold]Available Laws[/]  [grey]None[/]");
        }

        var table = new Table
        {
            Border = TableBorder.Simple,
            Expand = true,
            Title = new TableTitle("[bold]Available Laws[/]"),
        };
        table.AddColumn(new TableColumn("[green]ID[/]"));
        table.AddColumn("Name");
        table.AddColumn("Effect");

        foreach (var law in available)
        {
            table.AddRow($"[green]{Esc(law.Id)}[/]", Esc(law.Name), Esc(law.Tooltip));
        }

        return table;
    }

    static IRenderable BuildAvailableOrders(DayStartViewModel vm)
    {
        if (vm.AvailableOrders.Count == 0 && vm.OrderCooldowns.Count == 0)
        {
            return new Markup("[bold]Available Orders[/]  [grey]None[/]");
        }

        var items = new List<IRenderable>();

        if (vm.AvailableOrders.Count > 0)
        {
            var table = new Table { Border = TableBorder.Simple, Expand = false };
            table.Title = new TableTitle("[bold]Available Orders[/] [grey](per-order cooldowns)[/]");
            table.AddColumn(new TableColumn("[orange1]ID[/]"));
            table.AddColumn("Name");
            table.AddColumn("Effect");
            table.AddColumn("CD");

            foreach (var order in vm.AvailableOrders)
            {
                table.AddRow($"[orange1]{Esc(order.Id)}[/]", Esc(order.Name), Esc(order.Tooltip),
                    $"{order.CooldownDays}d");
            }

            items.Add(table);
        }

        if (vm.OrderCooldowns.Count > 0)
        {
            var cdList = string.Join(", ",
                vm.OrderCooldowns.Select(c => $"{Esc(c.OrderName)} ({c.DaysRemaining}d)"));
            items.Add(new Markup($"  [yellow]Order cooldowns: {cdList}[/]"));
        }

        return new Rows(items);
    }

    static IRenderable BuildAvailableMissions(DayStartViewModel vm)
    {
        if (vm.AvailableMissions.Count == 0)
        {
            return new Markup("[bold]Available Missions[/]  [grey]None[/]");
        }

        var table = new Table { Border = TableBorder.Simple, Expand = false };
        table.Title = new TableTitle("[bold]Available Missions[/]");
        table.AddColumn(new TableColumn("[dodgerblue1]ID[/]"));
        table.AddColumn("Name");
        table.AddColumn("Duration");
        table.AddColumn("Workers");
        table.AddColumn("Effect");

        foreach (var mission in vm.AvailableMissions)
        {
            table.AddRow(
                $"[dodgerblue1]{Esc(mission.Id)}[/]",
                Esc(mission.Name),
                $"{mission.DurationDays}d",
                $"{mission.RequiredIdleWorkers} wkrs",
                Esc(mission.Tooltip));
        }

        return table;
    }

    static IRenderable BuildCommandPanel()
    {
        return new Panel(
            new Markup(
                "[bold]assign[/] [grey]<Job> <N>[/]  [grey]|[/]  " +
                "[bold]enact[/] [grey]<LawId>[/]  [grey]|[/]  " +
                "[bold]order[/] [grey]<OrderId>[/]  [grey]|[/]  " +
                "[bold]mission[/] [grey]<MissionId>[/]  [grey]|[/]  " +
                "[bold]clear_assignments[/]  [grey]|[/]  " +
                "[bold]clear_action[/]  [grey]|[/]  " +
                "[bold]end_day[/]  [grey]|[/]  " +
                "[bold]view[/] [grey]<tab>[/]  [grey]|[/]"))
        {
            Header = new PanelHeader("Commands (<> = required)"),
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 0),
        };
    }

    public void RenderPendingDayAction(PendingPlanViewModel vm)
    {
        if (vm.QueuedActionType is null)
        {
            console.MarkupLine("Action: [grey]none[/]");
        }
        else
        {
            console.MarkupLine(
                $"Action: [cyan]{Esc(vm.QueuedActionType)}[/] -> [bold]{Esc(vm.QueuedActionName ?? "")}[/]");
        }
    }

    public void RenderDayReport(DayReportViewModel vm)
    {
        Clear();
        console.WriteLine();
        console.Write(new Rule($"[bold]Day {vm.Day} Resolution[/]") { Style = Style.Parse("blue") });

        if (vm.DeltaSummary is not null)
        {
            console.MarkupLine(Esc(vm.DeltaSummary));
        }

        console.WriteLine();

        foreach (var entry in vm.Entries)
        {
            if (entry.Messages.Count == 0) continue;
            TypewriteLine($"[bold yellow]{Esc(entry.Title)}[/]");
            foreach (var msg in entry.Messages)
                TypewriteLine($"  {Esc(msg)}");
        }

        if (vm.TriggeredEvents.Count > 0)
        {
            TypewriteLine($"[bold orange1]Events:[/] {Esc(string.Join(", ", vm.TriggeredEvents))}");
        }

        if (vm.EventResponses.Count > 0)
        {
            foreach (var resp in vm.EventResponses)
            {
                TypewriteLine($"[bold orange1]Response:[/] {Esc(resp.EventName)} -> {Esc(resp.ChosenResponse)}");
            }
        }

        if (vm.ResolvedMissions.Count > 0)
        {
            TypewriteLine($"[bold dodgerblue1]Missions:[/] {Esc(string.Join(", ", vm.ResolvedMissions))}");
        }

        if (vm.RecoveryEnabledToday)
        {
            TypewriteLine(
                $"[green]Recovery:[/] recovered {vm.RecoveredWorkersToday} workers (medicine -{vm.RecoveryMedicineSpentToday})");
        }
        else
        {
            TypewriteLine($"[yellow]Recovery:[/] blocked — {Esc(vm.RecoveryBlockedReason ?? "unknown")}");
        }

        if (vm.AllocationAlert is not null)
        {
            console.WriteLine();
            console.MarkupLine($"[bold yellow]{Esc(vm.AllocationAlert)}[/]");
        }

        console.WriteLine();
        console.MarkupLine("[grey]Press any key to continue...[/]");
        Console.ReadKey(true);
    }

    void TypewriteLine(string markup)
    {
        // Strip Spectre markup to get plain text length for pacing
        var plain = Markup.Remove(markup);
        foreach (var ch in plain)
        {
            Console.Write(ch);
            Thread.Sleep(ch == ' ' ? 5 : 10);
        }

        // Overwrite the plain-text line with the fully styled markup version
        Console.Write('\r');
        console.MarkupLine(markup);
    }

    public void RenderFinal(GameOverViewModel vm)
    {
        console.WriteLine();

        if (vm.Survived)
        {
            var content = new Rows(
                new Markup(
                    $"You endured to Day {GameBalance.TargetSurvivalDay}. The city survives, but at great cost."),
                new Markup(
                    $"Total deaths: [red]{vm.TotalDeaths}[/], total desertions: [yellow]{vm.TotalDesertions}[/]"),
                new Markup("Final Morale, Unrest, Sickness shown on next screen.")
            );
            var panel = new Panel(content)
            {
                Header = new PanelHeader("[bold green]VICTORY[/]"),
                Border = BoxBorder.Double,
                BorderStyle = Style.Parse("green"),
                Padding = new Padding(2, 1),
            };
            console.Write(panel);
        }
        else
        {
            var lines = new List<Markup>
            {
                new($"[bold]Game Over on Day {vm.Day}:[/] {Esc(vm.Cause.ToString())}"),
            };
            if (!string.IsNullOrWhiteSpace(vm.Details))
            {
                lines.Add(new Markup(Esc(vm.Details)));
            }

            lines.Add(new Markup(
                $"Total deaths: [red]{vm.TotalDeaths}[/], total desertions: [yellow]{vm.TotalDesertions}[/]"));
            lines.Add(new Markup("Final Morale, Unrest, Sickness shown on next screen."));

            var panel = new Panel(new Rows(lines))
            {
                Header = new PanelHeader("[bold red]DEFEAT[/]"),
                Border = BoxBorder.Double,
                BorderStyle = Style.Parse("red"),
                Padding = new Padding(2, 1),
            };
            console.Write(panel);
        }

        console.WriteLine();
    }

    public void RenderEventPrompt(PendingEventResponse pending)
    {
        console.WriteLine();
        console.Write(new Rule($"[bold orange1]EVENT: {Esc(pending.Event.Name)}[/]")
            { Style = Style.Parse("orange1") });
        console.MarkupLine($"  {Esc(pending.Event.Description)}");
        console.WriteLine();

        for (var i = 0; i < pending.Responses.Count; i++)
        {
            var r = pending.Responses[i];
            var tooltip = r.Tooltip is not null ? $" [grey]({Esc(r.Tooltip)})[/]" : "";
            console.MarkupLine($"  [bold]{i + 1}.[/] {Esc(r.Label)}{tooltip}");
        }

        console.WriteLine();
    }

    static string FormatDelta(int delta)
    {
        return delta switch
        {
            > 0 => $"+{delta}/d",
            < 0 => $"{delta}/d",
            _ => "0/d",
        };
    }

    static string MoraleMarkup(int morale)
    {
        return morale switch
        {
            < 25 => $"[bold red]{morale}[/]",
            < 50 => $"[yellow]{morale}[/]",
            _ => $"[green]{morale}[/]",
        };
    }

    static string UnrestMarkup(int unrest)
    {
        return unrest switch
        {
            > 70 => $"[bold red]{unrest}[/]",
            > 50 => $"[yellow]{unrest}[/]",
            _ => $"[green]{unrest}[/]",
        };
    }

    static string SicknessMarkup(int sickness)
    {
        return sickness switch
        {
            > 70 => $"[bold red]{sickness}[/]",
            >= 50 => $"[yellow]{sickness}[/]",
            _ => $"[green]{sickness}[/]",
        };
    }

    static string SicknessStatusNote(int sickness)
    {
        return sickness switch
        {
            > 70 => "[bold red][[recovery LOCKED | deaths each day]][/]",
            >= 50 => "[yellow][[recovery LOCKED at ≥50]][/]",
            _ => "[green][[recovery enabled]][/]",
        };
    }

    static string GetTagColor(string tag)
    {
        return tag switch
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
            _ => "white",
        };
    }
}