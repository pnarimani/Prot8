using Prot8.Cli.Commands;
using Prot8.Cli.Output;
using Prot8.Cli.ViewModels;
using Prot8.Jobs;
using Prot8.Simulation;

namespace Prot8.Cli.Input;

public sealed class ConsoleInputReader(CommandParser parser)
{
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
        var action = new TurnActionChoice();

        var pendingPlanVm = GameStateToViewModels.ToPendingPlanViewModel(action);
        renderer.RenderPendingDayAction(pendingPlanVm);

        while (true)
        {
            var raw = Console.ReadLine();
            if (raw is null)
            {
                FinalizeAllocation(state, allocation);
                return new DayCommandPlan(allocation, action);
            }

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
                        PrintInvalidAndHelp(renderer, state, "help takes no parameters.");
                        break;
                    }

                    var helpVm = GameStateToViewModels.ToDayStartViewModel(state);
                    renderer.RenderActionReference(helpVm);
                    break;

                default:
                    if (!parser.TryParse(trimmed, out var parsed, out var parseError))
                    {
                        PrintInvalidAndHelp(renderer, state, parseError);
                        break;
                    }

                    var context = new CommandContext(state, action);
                    var result = parsed!.Execute(context);
                    action = context.Action;

                    if (result.Success)
                    {
                        Console.WriteLine(result.Message);
                        if (result.EndDayRequested)
                        {
                            FinalizeAllocation(state, allocation);
                            return new DayCommandPlan(allocation, action);
                        }
                    }
                    else
                    {
                        PrintInvalidAndHelp(renderer, state, result.Message);
                    }

                    break;
            }
        }
    }

    public static void FinalizeAllocation(GameState state, JobAllocation allocation)
    {
        var available = state.AvailableHealthyWorkersForAllocation;
        var assigned = allocation.TotalAssigned();
        if (assigned > available)
            throw new InvalidOperationException("Assigned workers exceed available workers.");

        allocation.SetIdleWorkers(available - assigned);
    }

    void PrintInvalidAndHelp(ConsoleRenderer renderer, GameState state, string message)
    {
        Console.WriteLine($"Invalid command: {message}");
        var helpVm = GameStateToViewModels.ToDayStartViewModel(state);
        renderer.RenderActionReference(helpVm);
    }
}