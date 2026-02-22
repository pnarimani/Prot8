using Prot8.Cli.Output;
using Prot8.Cli.ViewModels;
using Prot8.Jobs;
using Prot8.Laws;
using Prot8.Missions;
using Prot8.Orders;
using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Cli.Input;

public sealed class ConsoleInputReader
{
    private static bool _noShortcuts;

    public ConsoleInputReader(bool noShortcuts)
    {
        _noShortcuts = noShortcuts;
    }

    public static bool TryExecuteCommand(GameState state, JobAllocation allocation, ref TurnActionChoice action,
        string rawCommand, out string message, out bool endDayRequested)
    {
        message = string.Empty;
        endDayRequested = false;

        if (string.IsNullOrWhiteSpace(rawCommand))
        {
            message = "Command cannot be empty.";
            return false;
        }

        var parts = rawCommand.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var command = parts[0].ToLowerInvariant();

        switch (command)
        {
            case "assign":
                return TryAssign(state, allocation, parts, out message);

            case "clear_assignments":
                if (parts.Length != 1)
                {
                    message = "clear_assignments takes no parameters.";
                    return false;
                }

                ClearAssignments(state, allocation);
                message = "All job assignments cleared.";
                return true;

            case "enact":
            case "enact_law":
                return TryQueueLaw(state, ref action, parts, out message);

            case "order":
            case "issue_order":
                return TryQueueOrder(state, ref action, parts, out message);

            case "mission":
            case "start_mission":
                return TryQueueMission(state, ref action, parts, out message);

            case "clear_action":
                if (parts.Length != 1)
                {
                    message = "clear_action takes no parameters.";
                    return false;
                }

                action = new TurnActionChoice();
                message = "Queued day action cleared.";
                return true;

            case "end_day":
                if (parts.Length != 1)
                {
                    message = "end_day takes no parameters.";
                    return false;
                }

                endDayRequested = true;
                message = "Day resolution requested.";
                return true;

            default:
                message = $"Unknown command '{parts[0]}'.";
                return false;
        }
    }

    public DayCommandPlan ReadDayPlan(GameState state, ConsoleRenderer renderer)
    {
        var allocation = BuildStartingAllocation(state, out var allocationAdjustmentMessage);
        var action = new TurnActionChoice();

        Console.WriteLine(
            "Command mode: enter actions with parameters. Type 'help' for all commands. End with 'end_day'.");
        if (!string.IsNullOrWhiteSpace(allocationAdjustmentMessage))
            Console.WriteLine(allocationAdjustmentMessage);

        var pendingPlanVm = GameStateToViewModels.ToPendingPlanViewModel(state, allocation, action, _noShortcuts);
        renderer.RenderPendingPlan(pendingPlanVm);

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
                case "show_plan":
                    if (parts.Length != 1)
                    {
                        PrintInvalidAndHelp(renderer, state, "show_plan takes no parameters.");
                        break;
                    }

                    var showPlanVm = GameStateToViewModels.ToPendingPlanViewModel(state, allocation, action, _noShortcuts);
                    renderer.RenderPendingPlan(showPlanVm);
                    break;

                case "help":
                    if (parts.Length != 1)
                    {
                        PrintInvalidAndHelp(renderer, state, "help takes no parameters.");
                        break;
                    }

                    var helpVm = GameStateToViewModels.ToDayStartViewModel(state, _noShortcuts);
                    renderer.RenderActionReference(helpVm);
                    break;

                case "end_day":
                case "assign":
                case "clear_assignments":
                case "enact":
                case "enact_law":
                case "order":
                case "issue_order":
                case "mission":
                case "start_mission":
                case "clear_action":
                    if (TryExecuteCommand(state, allocation, ref action, trimmed, out var commandMessage,
                            out var endDayRequested))
                    {
                        Console.WriteLine(commandMessage);
                        if (endDayRequested)
                        {
                            FinalizeAllocation(state, allocation);
                            return new DayCommandPlan(allocation, action);
                        }
                    }
                    else
                    {
                        PrintInvalidAndHelp(renderer, state, commandMessage);
                    }

                    break;

                default:
                    PrintInvalidAndHelp(renderer, state, $"Unknown command '{parts[0]}'.");
                    break;
            }
        }
    }

    static bool TryAssign(GameState state, JobAllocation allocation, string[] parts, out string message)
    {
        if (parts.Length != 3)
        {
            var jobRef = _noShortcuts ? "JobType" : "JobRef|JobType";
            message = $"Usage: assign <{jobRef}> <Workers>.";
            return false;
        }

        if (!TryResolveJob(parts[1], out var job, out var jobReason))
        {
            message = jobReason;
            return false;
        }

        if (!int.TryParse(parts[2], out var workers))
        {
            message = "Workers must be a whole number.";
            return false;
        }

        if (workers < 0)
        {
            message = "Workers cannot be negative.";
            return false;
        }

        if (workers % JobAllocation.Step != 0)
        {
            message = $"Workers must be in increments of {JobAllocation.Step}.";
            return false;
        }

        var available = state.AvailableHealthyWorkersForAllocation;
        var currentForJob = allocation.Workers[job];
        var newTotalAssigned = allocation.TotalAssigned() - currentForJob + workers;
        if (newTotalAssigned > available)
        {
            message = $"Assignment exceeds available workers ({newTotalAssigned}/{available}).";
            return false;
        }

        allocation.SetWorkers(job, workers);
        allocation.SetIdleWorkers(available - newTotalAssigned);
        message =
            $"Assigned {workers} workers to {job}. Total assigned: {newTotalAssigned}/{available}. Idle: {available - newTotalAssigned}.";
        return true;
    }

    static bool TryQueueLaw(GameState state, ref TurnActionChoice action, string[] parts, out string message)
    {
        if (parts.Length != 2)
        {
            var lawRef = _noShortcuts ? "LawId" : "LawRef|LawId";
            message = $"Usage: enact <{lawRef}>.";
            return false;
        }

        if (!TryResolveLaw(state, parts[1], out var law, out var reason))
        {
            message = reason;
            return false;
        }

        action = new TurnActionChoice { LawId = law!.Id };
        message = $"Queued law for today: {law.Name}.";
        return true;
    }

    static bool TryQueueOrder(GameState state, ref TurnActionChoice action, string[] parts, out string message)
    {
        if (parts.Length < 2 || parts.Length > 3)
        {
            var orderRef = _noShortcuts ? "OrderId" : "OrderRef|OrderId";
            message = $"Usage: order <{orderRef}> [ZoneId].";
            return false;
        }

        if (!TryResolveOrder(state, parts[1], out var order, out var reason))
        {
            message = reason;
            return false;
        }

        ZoneId? zone = null;
        if (parts.Length == 3)
        {
            if (!TryParseZone(parts[2], out var parsedZone))
            {
                message = $"Unknown ZoneId '{parts[2]}'.";
                return false;
            }

            zone = parsedZone;
        }

        if (order!.RequiresZoneSelection && !zone.HasValue)
        {
            message = $"{order.Name} requires a ZoneId parameter.";
            return false;
        }

        if (!order.RequiresZoneSelection && zone.HasValue)
        {
            message = $"{order.Name} does not accept a ZoneId parameter.";
            return false;
        }

        if (!order.CanIssue(state, zone, out reason))
        {
            message = $"Cannot issue {order.Name}: {reason}";
            return false;
        }

        action = new TurnActionChoice
        {
            EmergencyOrderId = order.Id,
            SelectedZoneForOrder = zone,
        };

        message = zone.HasValue
            ? $"Queued emergency order for today: {order.Name} ({zone.Value})."
            : $"Queued emergency order for today: {order.Name}.";
        return true;
    }

    static bool TryQueueMission(GameState state, ref TurnActionChoice action, string[] parts, out string message)
    {
        if (parts.Length != 2)
        {
            var missionRef = _noShortcuts ? "MissionId" : "MissionRef|MissionId";
            message = $"Usage: mission <{missionRef}>.";
            return false;
        }

        if (!TryResolveMission(state, parts[1], out var mission, out var reason))
        {
            message = reason;
            return false;
        }

        action = new TurnActionChoice { MissionId = mission!.Id };
        message = $"Queued mission for today: {mission.Name}.";
        return true;
    }

    static bool TryResolveLaw(GameState state, string token, out ILaw? law, out string reason)
    {
        law = null;
        var available = ActionAvailability.GetAvailableLaws(state);

        if (TryParseShortcut(token, 'l', out var shortcutIndex))
        {
            if (shortcutIndex < 1 || shortcutIndex > available.Count)
            {
                reason = $"Law shortcut '{token}' is out of range for currently available laws.";
                return false;
            }

            law = available[shortcutIndex - 1];
            reason = string.Empty;
            return true;
        }

        foreach (var item in available)
        {
            if (string.Equals(item.Id, token, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(Normalize(item.Name), Normalize(token), StringComparison.Ordinal))
            {
                law = item;
                reason = string.Empty;
                return true;
            }
        }

        reason = $"Law '{token}' is not currently available.";
        return false;
    }

    static bool TryResolveOrder(GameState state, string token, out IEmergencyOrder? order, out string reason)
    {
        order = null;
        var available = ActionAvailability.GetAvailableOrders(state);

        if (TryParseShortcut(token, 'o', out var shortcutIndex))
        {
            if (shortcutIndex < 1 || shortcutIndex > available.Count)
            {
                reason = $"Order shortcut '{token}' is out of range for currently available orders.";
                return false;
            }

            order = available[shortcutIndex - 1];
            reason = string.Empty;
            return true;
        }

        foreach (var item in available)
        {
            if (string.Equals(item.Id, token, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(Normalize(item.Name), Normalize(token), StringComparison.Ordinal))
            {
                order = item;
                reason = string.Empty;
                return true;
            }
        }

        reason = $"Order '{token}' is not currently available.";
        return false;
    }

    static bool TryResolveMission(GameState state, string token, out IMissionDefinition? mission, out string reason)
    {
        mission = null;
        var available = ActionAvailability.GetAvailableMissions(state);

        if (TryParseShortcut(token, 'm', out var shortcutIndex))
        {
            if (shortcutIndex < 1 || shortcutIndex > available.Count)
            {
                reason = $"Mission shortcut '{token}' is out of range for currently available missions.";
                return false;
            }

            mission = available[shortcutIndex - 1];
            reason = string.Empty;
            return true;
        }

        foreach (var item in available)
        {
            if (string.Equals(item.Id, token, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(Normalize(item.Name), Normalize(token), StringComparison.Ordinal))
            {
                mission = item;
                reason = string.Empty;
                return true;
            }
        }

        reason = $"Mission '{token}' is not currently available.";
        return false;
    }

    static bool TryResolveJob(string token, out JobType job, out string reason)
    {
        if (Enum.TryParse(token, true, out job))
        {
            reason = string.Empty;
            return true;
        }

        if (TryParseShortcut(token, 'j', out var shortcutIndex))
        {
            var jobs = ActionAvailability.GetJobTypes();
            if (shortcutIndex >= 1 && shortcutIndex <= jobs.Count)
            {
                job = jobs[shortcutIndex - 1];
                reason = string.Empty;
                return true;
            }

            reason = $"Job shortcut '{token}' is out of range.";
            return false;
        }

        reason = _noShortcuts
            ? $"Unknown JobType '{token}'."
            : $"Unknown JobRef '{token}'. Use JobType names or j1..j{ActionAvailability.GetJobTypes().Count}.";
        return false;
    }

    static bool TryParseShortcut(string token, char prefix, out int index)
    {
        index = 0;
        if (_noShortcuts)
            return false;

        if (token.Length < 2 || char.ToLowerInvariant(token[0]) != char.ToLowerInvariant(prefix))
            return false;

        return int.TryParse(token.Substring(1), out index);
    }

    static bool TryParseZone(string token, out ZoneId zone)
    {
        if (Enum.TryParse(token, true, out zone))
            return true;

        if (int.TryParse(token, out var raw) && raw >= (int)ZoneId.OuterFarms && raw <= (int)ZoneId.Keep)
        {
            zone = (ZoneId)raw;
            return true;
        }

        return false;
    }

    static string Normalize(string value)
    {
        var chars = value.ToLowerInvariant().ToCharArray();
        var buffer = new char[chars.Length];
        var idx = 0;
        foreach (var ch in chars)
        {
            if (char.IsLetterOrDigit(ch))
                buffer[idx++] = ch;
        }

        return new string(buffer, 0, idx);
    }

    static void ClearAssignments(GameState state, JobAllocation allocation)
    {
        foreach (var job in Enum.GetValues<JobType>())
            allocation.SetWorkers(job, 0);

        allocation.SetIdleWorkers(state.AvailableHealthyWorkersForAllocation);
    }

    public static void FinalizeAllocation(GameState state, JobAllocation allocation)
    {
        var available = state.AvailableHealthyWorkersForAllocation;
        var assigned = allocation.TotalAssigned();
        if (assigned > available)
            throw new InvalidOperationException("Assigned workers exceed available workers.");

        allocation.SetIdleWorkers(available - assigned);
    }

    static void PrintInvalidAndHelp(ConsoleRenderer renderer, GameState state, string message)
    {
        Console.WriteLine($"Invalid command: {message}");
        var helpVm = GameStateToViewModels.ToDayStartViewModel(state, _noShortcuts);
        renderer.RenderActionReference(helpVm);
    }

    public static JobAllocation BuildStartingAllocation(GameState state, out string? adjustmentMessage)
    {
        var allocation = new JobAllocation();
        foreach (var job in Enum.GetValues<JobType>())
            allocation.SetWorkers(job, state.Allocation.Workers[job]);

        adjustmentMessage = null;
        var available = state.AvailableHealthyWorkersForAllocation;
        if (allocation.TotalAssigned() > available)
        {
            var overflow = allocation.TotalAssigned() - available;
            foreach (var job in Enum.GetValues<JobType>().Reverse())
            {
                if (overflow <= 0)
                    break;

                var assigned = allocation.Workers[job];
                if (assigned <= 0)
                    continue;

                var reduction = Math.Min(assigned,
                    (overflow + JobAllocation.Step - 1) / JobAllocation.Step * JobAllocation.Step);
                allocation.SetWorkers(job, assigned - reduction);
                overflow -= reduction;
            }

            adjustmentMessage = "Previous assignments exceeded available workers today and were automatically reduced.";
        }

        allocation.SetIdleWorkers(available - allocation.TotalAssigned());
        return allocation;
    }
}