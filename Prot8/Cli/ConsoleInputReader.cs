using Prot8.Buildings;
using Prot8.Cli.Commands;
using Prot8.Cli.Output;
using Prot8.Cli.ViewModels;
using Prot8.Constants;
using Prot8.Events;
using Prot8.Scavenging;
using Prot8.Simulation;

namespace Prot8.Cli.Input;

public sealed class ConsoleInputReader(GameState state, GameViewModelFactory vmFactory, CommandParser parser)
{
    ActionTab _activeTab = ActionTab.Laws;

    public DayCommandPlan ReadDayPlan(ConsoleRenderer renderer)
    {
        var allocation = state.Allocation;

        // Auto-adjust allocation if workers were lost
        if (GameBalance.AllocationMode == WorkerAllocationMode.ManualAssignment)
        {
            var totalAssigned = allocation.TotalAssigned();
            var available = state.AvailableHealthyWorkersForAllocation;
            if (totalAssigned > available)
            {
                allocation.RemoveWorkersProportionally(totalAssigned - available);
            }
        }
        else
        {
            WorkerAllocationStrategy.ApplyAutomaticAllocation(state);
        }

        var action = new TurnActionChoice();

        var pendingPlanVm = GameViewModelFactory.ToPendingPlanViewModel(action);
        renderer.RenderPendingDayAction(pendingPlanVm);

        var currentVm = vmFactory.CreateDayStartViewModel();

        while (true)
        {
            var (raw, tabSwitch, resized) = TabCompletingReadLine.ReadLine(currentVm, _activeTab);

            if (resized)
            {
                currentVm = ReRender(renderer, action);
                continue;
            }

            // Tab auto-switch: user pressed Tab after a command prefix like "enact "
            if (tabSwitch.HasValue)
            {
                _activeTab = tabSwitch.Value;
                currentVm = ReRender(renderer, action);
                continue;
            }

            if (raw is null)
            {
                return new DayCommandPlan(action);
            }

            var trimmed = raw.Trim();
            if (trimmed.Length == 0)
            {
                continue;
            }

            var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0].ToLowerInvariant();

            switch (command)
            {
                case "view":
                    if (parts.Length != 2)
                    {
                        Console.WriteLine("Usage: view <laws|orders|missions>");
                        break;
                    }

                    if (!TryParseTab(parts[1], out var tab))
                    {
                        Console.WriteLine($"Unknown tab: {parts[1]}. Use: laws, orders, missions");
                        break;
                    }

                    _activeTab = tab;
                    currentVm = ReRender(renderer, action);
                    break;

                default:
                    if (!parser.TryParse(trimmed, out var parsed, out var parseError))
                    {
                        PrintInvalidAndHelp(parseError);
                        break;
                    }

                    var context = new CommandContext(state, action);
                    var result = parsed!.Execute(context);
                    action = context.Action;

                    if (result.Success)
                    {
                        if (result.EndDayRequested)
                        {
                            return new DayCommandPlan(action);
                        }

                        currentVm = ReRender(renderer, action);
                        Console.WriteLine(result.Message);
                    }
                    else
                    {
                        PrintInvalidAndHelp(result.Message);
                    }

                    break;
            }
        }
    }

    DayStartViewModel ReRender(ConsoleRenderer renderer, TurnActionChoice action)
    {
        renderer.Clear();
        var vm = new GameViewModelFactory(state).CreateDayStartViewModel();
        renderer.RenderDayStart(vm, _activeTab);
        var pendingVm = GameViewModelFactory.ToPendingPlanViewModel(action);
        renderer.RenderPendingDayAction(pendingVm);
        return vm;
    }

    static void PrintInvalidAndHelp(string message)
    {
        Console.WriteLine($"Invalid command: {message}");
    }

    public EventResponseChoice ReadEventResponses(PendingEvent pending)
    {
        ConsoleRenderer.RenderEventPrompt(pending);

        if (pending.Responses == null)
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            return new EventResponseChoice(pending.Event.Id, "");
        }
        
        Console.Write("Enter your choice (or press Enter for default): ");

        var line = Console.ReadLine()?.Trim();
        string responseId;

        if (string.IsNullOrEmpty(line))
        {
            responseId = pending.Responses[^1].Id;
        }
        else if (int.TryParse(line, out var num) && num >= 1 && num <= pending.Responses.Count)
        {
            responseId = pending.Responses[num - 1].Id;
        }
        else
        {
            responseId = pending.Responses[^1].Id;
        }

        return new EventResponseChoice(pending.Event.Id, responseId);
    }

    public NightPlan ReadNightPlan(ConsoleRenderer renderer)
    {
        var nightVm = vmFactory.CreateNightPhaseViewModel();
        renderer.RenderNightPhase(nightVm);

        if (nightVm.Locations.Count == 0)
        {
            return new NightPlan { SelectedLocationId = null, AssignedWorkers = 0 };
        }

        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(line) || line.Equals("skip", StringComparison.OrdinalIgnoreCase)
                                           || line.Equals("hunker", StringComparison.OrdinalIgnoreCase))
            {
                return new NightPlan { SelectedLocationId = null, AssignedWorkers = 0 };
            }

            if (!int.TryParse(line, out var locId))
            {
                Console.WriteLine("Enter a location ID number or 'skip'.");
                continue;
            }

            var loc = nightVm.Locations.FirstOrDefault(l => l.Id == locId);
            if (loc is null)
            {
                Console.WriteLine($"No location with ID {locId}. Try again.");
                continue;
            }

            Console.Write($"Workers to send ({nightVm.MinWorkers}-{nightVm.MaxWorkers}): ");
            var workerLine = Console.ReadLine()?.Trim();
            if (!int.TryParse(workerLine, out var workers))
                workers = nightVm.MinWorkers;

            workers = Math.Clamp(workers, nightVm.MinWorkers, Math.Min(nightVm.MaxWorkers, nightVm.AvailableWorkers));
            return new NightPlan { SelectedLocationId = locId, AssignedWorkers = workers };
        }
    }

    static bool TryParseTab(string input, out ActionTab tab)
    {
        tab = input.ToLowerInvariant() switch
        {
            "laws" => ActionTab.Laws,
            "orders" => ActionTab.Orders,
            "missions" => ActionTab.Missions,
            _ => (ActionTab)(-1),
        };
        return (int)tab >= 0;
    }
}