using Prot8.Cli.Commands;
using Prot8.Cli.Output;
using Prot8.Cli.ViewModels;
using Prot8.Events;
using Prot8.Jobs;
using Prot8.Simulation;

namespace Prot8.Cli.Input;

public sealed class ConsoleInputReader(CommandParser parser)
{
    ActionTab _activeTab = ActionTab.Laws;

    public bool TryExecuteCommand(GameState state, ref TurnActionChoice action, string rawCommand, out string message, out bool endDayRequested)
    {
        message = string.Empty;
        endDayRequested = false;

        if (!parser.TryParse(rawCommand, out var command, out var parseError))
        {
            message = parseError;
            return false;
        }

        var context = new CommandContext(state, action);
        var result = command!.Execute(context);
        action = context.Action;
        message = result.Message;
        endDayRequested = result.EndDayRequested;
        return result.Success;
    }

    public DayCommandPlan ReadDayPlan(GameState state, ConsoleRenderer renderer)
    {
        var allocation = state.Allocation;

        // Auto-adjust allocation if workers were lost
        var totalAssigned = allocation.TotalAssigned();
        var available = state.AvailableHealthyWorkersForAllocation;
        if (totalAssigned > available)
        {
            allocation.RemoveWorkersProportionally(totalAssigned - available);
        }

        var action = new TurnActionChoice();

        var pendingPlanVm = GameViewModelFactory.ToPendingPlanViewModel(action);
        renderer.RenderPendingDayAction(pendingPlanVm);

        var currentVm = new GameViewModelFactory(state).Create();

        while (true)
        {
            var (raw, tabSwitch, resized) = TabCompletingReadLine.ReadLine(currentVm, _activeTab);

            if (resized)
            {
                currentVm = ReRender(state, renderer, action);
                continue;
            }

            // Tab auto-switch: user pressed Tab after a command prefix like "enact "
            if (tabSwitch.HasValue)
            {
                _activeTab = tabSwitch.Value;
                currentVm = ReRender(state, renderer, action);
                continue;
            }

            if (raw is null)
                return new DayCommandPlan(allocation, action);

            var trimmed = raw.Trim();
            if (trimmed.Length == 0)
                continue;

            var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0].ToLowerInvariant();

            switch (command)
            {
                case "help":
                    if (parts.Length != 1)
                    {
                        PrintInvalidAndHelp(renderer, currentVm, "help takes no parameters.");
                        break;
                    }

                    renderer.RenderActionReference(currentVm);
                    break;

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
                    currentVm = ReRender(state, renderer, action);
                    break;

                default:
                    if (!parser.TryParse(trimmed, out var parsed, out var parseError))
                    {
                        PrintInvalidAndHelp(renderer, currentVm, parseError);
                        break;
                    }

                    var context = new CommandContext(state, action);
                    var result = parsed!.Execute(context);
                    action = context.Action;

                    if (result.Success)
                    {
                        if (result.EndDayRequested)
                        {
                            return new DayCommandPlan(allocation, action);
                        }

                        currentVm = ReRender(state, renderer, action);
                        Console.WriteLine(result.Message);
                    }
                    else
                    {
                        PrintInvalidAndHelp(renderer, currentVm, result.Message);
                    }

                    break;
            }
        }
    }

    DayStartViewModel ReRender(GameState state, ConsoleRenderer renderer, TurnActionChoice action)
    {
        renderer.Clear();
        var vm = new GameViewModelFactory(state).Create();
        renderer.RenderDayStart(vm, _activeTab);
        var pendingVm = GameViewModelFactory.ToPendingPlanViewModel(action);
        renderer.RenderPendingDayAction(pendingVm);
        return vm;
    }

    static void PrintInvalidAndHelp(ConsoleRenderer renderer, DayStartViewModel vm, string message)
    {
        Console.WriteLine($"Invalid command: {message}");
        renderer.RenderActionReference(vm);
    }

    public List<EventResponseChoice> ReadEventResponses(IReadOnlyList<PendingEventResponse> pendingResponses, ConsoleRenderer renderer)
    {
        var choices = new List<EventResponseChoice>();

        foreach (var pending in pendingResponses)
        {
            renderer.RenderEventPrompt(pending);
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

            choices.Add(new EventResponseChoice(pending.Event.Id, responseId));
        }

        return choices;
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
