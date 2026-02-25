using Prot8.Cli.ViewModels;
using Prot8.Constants;
using Prot8.Events;
using Prot8.Simulation;
using Prot8.Zones;
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
        Clear();
        console.Write(
            new Rows(
                BuildHeader(vm),
                new Columns(BuildResourcesTable(vm), BuildPopulationTable(vm)),
                BuildStatusWarnings(vm),
                new Columns(BuildBuildingsTable(vm), BuildZonesTable(vm)),
                new Rule { Style = Style.Parse("grey") },
                BuildStateSection(vm),
                new Rule { Style = Style.Parse("grey") },
                vm.CurrentEvent == null ? BuildActionsSection(vm, activeTab) : new Text(""),
                vm.CurrentEvent == null ? BuildCommandPanel() : new Text(""),
                vm.CurrentEvent != null ? RenderEventPrompt(vm.CurrentEvent) : new Text("")
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

        if (vm.GlobalProductionMultiplier < 1.0 || vm.GlobalProductionMultiplier > 1.0)
        {
            var color = vm.GlobalProductionMultiplier < 1.0 ? "yellow" : "green";
            var breakdown = vm.ProductionMultiplierBreakdown.Count > 0
                ? " (" + string.Join(", ", vm.ProductionMultiplierBreakdown.Select(e => $"{Esc(e.Source)}: {e.Value:F2}x")) + ")"
                : "";
            items.Add(new Markup($"  [{color}]Production: {vm.GlobalProductionMultiplier:F2}x{breakdown}[/]"));
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

        var foodNeedStr = FormatConsumptionWithBreakdown(foodNeed, vm.FoodConsumptionBreakdown);
        var waterNeedStr = FormatConsumptionWithBreakdown(waterNeed, vm.WaterConsumptionBreakdown);

        var moraleBreakdownStr = FormatDeltaBreakdown(vm.MoraleDeltaBreakdown);
        var unrestBreakdownStr = FormatDeltaBreakdown(vm.UnrestDeltaBreakdown);
        var sicknessBreakdownStr = FormatDeltaBreakdown(vm.SicknessDeltaBreakdown);

        table.AddRow(
            $"[grey]{foodNeedStr}[/]",
            $"[grey]{waterNeedStr}[/]",
            $"[grey]~{fuelNeed}/d[/]",
            "[grey]-[/]",
            "[grey]-[/]",
            $"[grey]{moraleBreakdownStr}[/]",
            $"[grey]{unrestBreakdownStr}[/]",
            $"[grey]{sicknessBreakdownStr}[/]");

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
        table.AddColumn(new TableColumn("[bold]Idle[/]").Centered());

        table.AddRow(
            pop.HealthyWorkers.ToString(),
            pop.Guards.ToString(),
            pop.SickWorkers.ToString(),
            pop.Elderly.ToString(),
            pop.TotalPopulation.ToString(),
            onMissions.ToString(),
            $"[bold]{available}[/]",
            $"[bold]{vm.IdleWorkersForAssignment}[/]");

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
                new Text(""), new Text(""), new Text(""), new Text(""));
        }

        table.Title = new TableTitle("[bold]Population[/]");
        return table;
    }

    static IRenderable BuildBuildingsTable(DayStartViewModel vm)
    {
        var table = new Table
        {
            Border = TableBorder.Horizontal,
            Expand = true,
            Title = new TableTitle("[bold]Buildings[/]"),
        };
        table.AddColumn("Zone");
        table.AddColumn("Building");
        table.AddColumn(new TableColumn("Workers").RightAligned());
        table.AddColumn("Input");
        table.AddColumn("Output");
        table.AddColumn("+Per Worker");

        string? lastZone = null;
        foreach (var bvm in vm.Buildings)
        {
            var zoneName = bvm.ZoneName == lastZone ? "" : bvm.ZoneName;
            lastZone = bvm.ZoneName;

            if (bvm.IsDestroyed)
            {
                table.AddRow(
                    $"[red]{Esc(zoneName)}[/]",
                    $"[red]{Esc(bvm.Name)} (DESTROYED)[/]",
                    "[red]-[/]",
                    "[red]-[/]",
                    "[red]-[/]",
                    "[red]-[/]");
                continue;
            }

            var inputs = string.Join(", ", bvm.CurrentInput.Select(x => x.ToString()));
            var outputs = string.Join(", ", bvm.CurrentOutput.Select(x => x.ToString()));
            var perWorker = string.Join(", ", bvm.OutputPerWorker.Select(x => x.ToString()));
            var inputStr = inputs.Length > 0 ? Esc(inputs) : " ";
            table.AddRow(
                Esc(zoneName),
                Esc(bvm.Name),
                $"{bvm.AssignedWorkers}/{bvm.MaxWorkers}",
                inputStr,
                Esc(outputs),
                $"+{Esc(perWorker)}");
        }

        return table;
    }

    static Table BuildZonesTable(DayStartViewModel vm)
    {
        var storageLookup = new Dictionary<ZoneId, ZoneStorageViewModel>();
        foreach (var zs in vm.ZoneStorages)
            storageLookup[zs.ZoneId] = zs;

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
        table.AddColumn(new TableColumn("Cap").RightAligned());
        table.AddColumn(new TableColumn("Pop").RightAligned());
        table.AddColumn(new TableColumn("Stor").RightAligned());
        table.AddColumn(new TableColumn("Food").RightAligned());
        table.AddColumn(new TableColumn("Water").RightAligned());
        table.AddColumn(new TableColumn("Fuel").RightAligned());
        table.AddColumn(new TableColumn("Med").RightAligned());
        table.AddColumn(new TableColumn("Mat").RightAligned());

        foreach (var zone in vm.Zones)
        {
            var nameMarkup = zone.IsLost ? $"[red]{Esc(zone.Name)}[/]" : Esc(zone.Name);

            if (zone.IsLost)
            {
                table.AddRow(
                    $"[red]{(int)zone.Id}[/]",
                    nameMarkup,
                    "[red]LOST[/]",
                    "[red]-[/]", "[red]-[/]", "[red]-[/]",
                    "[red]-[/]", "[red]-[/]", "[red]-[/]",
                    "[red]-[/]", "[red]-[/]", "[red]-[/]");
                continue;
            }

            var integrityColor = zone.Integrity switch
            {
                <= 25 => "bold red",
                <= 50 => "yellow",
                _ => "green",
            };
            var over = zone.Population - zone.Capacity;
            var overText = over > 0 ? $" [red]+{over}[/]" : "";

            var storLvl = "";
            var food = "-"; var water = "-"; var fuel = "-"; var med = "-"; var mat = "-";
            if (storageLookup.TryGetValue(zone.Id, out var zs))
            {
                storLvl = $"{zs.Level}/{zs.MaxLevel} ({zs.CapacityPerResource})";
                food = zs.Food.ToString();
                water = zs.Water.ToString();
                fuel = zs.Fuel.ToString();
                med = zs.Medicine.ToString();
                mat = zs.Materials.ToString();
            }

            table.AddRow(
                ((int)zone.Id).ToString(),
                nameMarkup,
                "[green]active[/]",
                $"[{integrityColor}]{zone.Integrity}[/]",
                zone.Capacity.ToString(),
                $"{zone.Population}{overText}",
                storLvl,
                food, water, fuel, med, mat);
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
                "[bold]assign[/] [grey]<Building> <N>[/]  [grey]|[/]  " +
                "[bold]enact[/] [grey]<LawId>[/]  [grey]|[/]  " +
                "[bold]order[/] [grey]<OrderId>[/]  [grey]|[/]  " +
                "[bold]mission[/] [grey]<MissionId>[/]  [grey]|[/]  " +
                "[bold]upgrade[/] [grey]<Zone>[/]  [grey]|[/]  " +
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

    public void RenderDayReport(DayStartViewModel dayStartVm, DayReportViewModel reportVm)
    {
        void RenderSlimDay()
        {
            console.Write(
                new Rows(
                    BuildHeader(dayStartVm),
                    new Columns(BuildResourcesTable(dayStartVm), BuildPopulationTable(dayStartVm)),
                    BuildStatusWarnings(dayStartVm),
                    new Columns(BuildBuildingsTable(dayStartVm), BuildZonesTable(dayStartVm)),
                    new Rule { Style = Style.Parse("grey") },
                    BuildStateSection(dayStartVm)
                )
            );
        }

        foreach (var entry in reportVm.Entries)
        {
            if (entry.Messages.Count == 0)
            {
                continue;
            }

            Clear();
            RenderSlimDay();
            console.WriteLine();
            console.Write(new Rule($"[bold]Day {reportVm.Day} Resolution[/]") { Style = Style.Parse("blue") });

            var text = entry.Messages.Aggregate("", (s, s1) => s + "\n" + s1) + "\n";
            console.Write(new Panel(new Text(text) { Justification = Justify.Center })
            {
                Header = new PanelHeader(entry.Title, Justify.Center),
                Border = BoxBorder.Rounded,
                Expand = true,
                Padding = new Padding(1, 10),
            });

            console.MarkupLine("[grey]Press any key to continue...[/]");
            Console.ReadKey(true);
        }

        Clear();
        RenderSlimDay();
        console.WriteLine();
        console.Write(new Rule($"[bold]Day {reportVm.Day} Resolution[/]") { Style = Style.Parse("blue") });

        TypewriteLine(
            reportVm.RecoveryEnabledToday
                ? $"[green]Recovery:[/] recovered {reportVm.RecoveredWorkersToday} workers (medicine -{reportVm.RecoveryMedicineSpentToday})"
                : $"[yellow]Recovery:[/] blocked — {Esc(reportVm.RecoveryBlockedReason ?? "unknown")}");

        if (reportVm.AllocationAlert is not null)
        {
            console.WriteLine();
            console.MarkupLine($"[bold yellow]{Esc(reportVm.AllocationAlert)}[/]");
        }

        if (reportVm.DeltaSummary is not null)
        {
            console.MarkupLine(Esc(reportVm.DeltaSummary));
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

    public static Panel RenderEventPrompt(PendingEvent pending)
    {
        var s = $"\n[italic]{Esc(pending.Event.Description)}[/]";
        s += "\n";
        if (pending.Responses != null)
        {
            for (var i = 0; i < pending.Responses.Count; i++)
            {
                var r = pending.Responses[i];
                var tooltip = r.Tooltip is not null ? $" [grey]({Esc(r.Tooltip)})[/]" : "";
                s += $"  [bold]{i + 1}.[/] {Esc(r.Label)}{tooltip}";
            }

            s += "\n";
        }

        var panel = new Panel(new Markup(s) { Justification = Justify.Center })
        {
            Header = new PanelHeader($"[bold orange1]{pending.Event.Name}[/]", Justify.Center),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("orange1"),
            Expand = true,
            Padding = new Padding(1, 2),
        };


        return panel;
    }

    static string FormatConsumptionWithBreakdown(int need, IReadOnlyList<MultiplierEntry> breakdown)
    {
        if (breakdown.Count == 0)
        {
            return $"~{need}/d";
        }

        var parts = string.Join(", ", breakdown.Select(e => $"{Esc(e.Source)}: {e.Value:F2}x"));
        return $"~{need}/d ({parts})";
    }

    static string FormatDeltaBreakdown(IReadOnlyList<DeltaEntry> breakdown)
    {
        if (breakdown.Count == 0)
        {
            return "-";
        }

        return string.Join(", ", breakdown.Select(e =>
        {
            var sign = e.Value >= 0 ? "+" : "";
            return $"{Esc(e.Source)} {sign}{e.Value}";
        }));
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