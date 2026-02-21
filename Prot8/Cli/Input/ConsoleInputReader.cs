using System;
using System.Linq;
using Prot8.Cli;
using Prot8.Cli.Output;
using Prot8.Jobs;
using Prot8.Laws;
using Prot8.Missions;
using Prot8.Orders;
using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Cli.Input;

public sealed class ConsoleInputReader
{
    public DayCommandPlan ReadDayPlan(GameState state, ConsoleRenderer renderer)
    {
        var allocation = BuildStartingAllocation(state, out var allocationAdjustmentMessage);
        var action = new TurnActionChoice();

        Console.WriteLine("Command mode: enter actions with parameters. Type 'help' for all commands. End with 'end_day'.");
        if (!string.IsNullOrWhiteSpace(allocationAdjustmentMessage))
        {
            Console.WriteLine(allocationAdjustmentMessage);
        }

        renderer.RenderPendingPlan(state, allocation, action);

        while (true)
        {
            Console.Write("cmd> ");
            var raw = Console.ReadLine();
            if (raw is null)
            {
                FinalizeAllocation(state, allocation);
                return new DayCommandPlan(allocation, action);
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
                case "assign":
                    if (TryAssign(state, allocation, parts, out var assignMessage))
                    {
                        Console.WriteLine(assignMessage);
                    }
                    else
                    {
                        PrintInvalidAndHelp(renderer, state, assignMessage);
                    }

                    break;

                case "clear_assignments":
                    if (parts.Length != 1)
                    {
                        PrintInvalidAndHelp(renderer, state, "clear_assignments takes no parameters.");
                        break;
                    }

                    ClearAssignments(state, allocation);
                    Console.WriteLine("All job assignments cleared.");
                    break;

                case "enact":
                case "enact_law":
                    if (TryQueueLaw(state, ref action, parts, out var lawMessage))
                    {
                        Console.WriteLine(lawMessage);
                    }
                    else
                    {
                        PrintInvalidAndHelp(renderer, state, lawMessage);
                    }

                    break;

                case "order":
                case "issue_order":
                    if (TryQueueOrder(state, ref action, parts, out var orderMessage))
                    {
                        Console.WriteLine(orderMessage);
                    }
                    else
                    {
                        PrintInvalidAndHelp(renderer, state, orderMessage);
                    }

                    break;

                case "mission":
                case "start_mission":
                    if (TryQueueMission(state, ref action, parts, out var missionMessage))
                    {
                        Console.WriteLine(missionMessage);
                    }
                    else
                    {
                        PrintInvalidAndHelp(renderer, state, missionMessage);
                    }

                    break;

                case "clear_action":
                    if (parts.Length != 1)
                    {
                        PrintInvalidAndHelp(renderer, state, "clear_action takes no parameters.");
                        break;
                    }

                    action = new TurnActionChoice();
                    Console.WriteLine("Queued day action cleared.");
                    break;

                case "show_plan":
                    if (parts.Length != 1)
                    {
                        PrintInvalidAndHelp(renderer, state, "show_plan takes no parameters.");
                        break;
                    }

                    renderer.RenderPendingPlan(state, allocation, action);
                    break;

                case "help":
                    if (parts.Length != 1)
                    {
                        PrintInvalidAndHelp(renderer, state, "help takes no parameters.");
                        break;
                    }

                    renderer.RenderActionReference(state);
                    break;

                case "end_day":
                    if (parts.Length != 1)
                    {
                        PrintInvalidAndHelp(renderer, state, "end_day takes no parameters.");
                        break;
                    }

                    FinalizeAllocation(state, allocation);
                    return new DayCommandPlan(allocation, action);

                default:
                    PrintInvalidAndHelp(renderer, state, $"Unknown command '{parts[0]}'.");
                    break;
            }
        }
    }

    private static bool TryAssign(GameState state, JobAllocation allocation, string[] parts, out string message)
    {
        if (parts.Length != 3)
        {
            message = "Usage: assign <JobRef|JobType> <Workers>.";
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
        message = $"Assigned {workers} workers to {job}. Total assigned: {newTotalAssigned}/{available}. Idle: {available - newTotalAssigned}.";
        return true;
    }

    private static bool TryQueueLaw(GameState state, ref TurnActionChoice action, string[] parts, out string message)
    {
        if (parts.Length != 2)
        {
            message = "Usage: enact <LawRef|LawId>.";
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

    private static bool TryQueueOrder(GameState state, ref TurnActionChoice action, string[] parts, out string message)
    {
        if (parts.Length < 2 || parts.Length > 3)
        {
            message = "Usage: order <OrderRef|OrderId> [ZoneId].";
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
            SelectedZoneForOrder = zone
        };

        message = zone.HasValue
            ? $"Queued emergency order for today: {order.Name} ({zone.Value})."
            : $"Queued emergency order for today: {order.Name}.";
        return true;
    }

    private static bool TryQueueMission(GameState state, ref TurnActionChoice action, string[] parts, out string message)
    {
        if (parts.Length != 2)
        {
            message = "Usage: mission <MissionRef|MissionId>.";
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

    private static bool TryResolveLaw(GameState state, string token, out ILaw? law, out string reason)
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
            if (string.Equals(item.Id, token, StringComparison.OrdinalIgnoreCase) || string.Equals(Normalize(item.Name), Normalize(token), StringComparison.Ordinal))
            {
                law = item;
                reason = string.Empty;
                return true;
            }
        }

        reason = $"Law '{token}' is not currently available.";
        return false;
    }

    private static bool TryResolveOrder(GameState state, string token, out IEmergencyOrder? order, out string reason)
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
            if (string.Equals(item.Id, token, StringComparison.OrdinalIgnoreCase) || string.Equals(Normalize(item.Name), Normalize(token), StringComparison.Ordinal))
            {
                order = item;
                reason = string.Empty;
                return true;
            }
        }

        reason = $"Order '{token}' is not currently available.";
        return false;
    }

    private static bool TryResolveMission(GameState state, string token, out IMissionDefinition? mission, out string reason)
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
            if (string.Equals(item.Id, token, StringComparison.OrdinalIgnoreCase) || string.Equals(Normalize(item.Name), Normalize(token), StringComparison.Ordinal))
            {
                mission = item;
                reason = string.Empty;
                return true;
            }
        }

        reason = $"Mission '{token}' is not currently available.";
        return false;
    }

    private static bool TryResolveJob(string token, out JobType job, out string reason)
    {
        if (Enum.TryParse<JobType>(token, true, out job))
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

        reason = $"Unknown JobRef '{token}'. Use JobType names or j1..j{ActionAvailability.GetJobTypes().Count}.";
        return false;
    }

    private static bool TryParseShortcut(string token, char prefix, out int index)
    {
        index = 0;
        if (token.Length < 2 || char.ToLowerInvariant(token[0]) != char.ToLowerInvariant(prefix))
        {
            return false;
        }

        return int.TryParse(token.Substring(1), out index);
    }

    private static bool TryParseZone(string token, out ZoneId zone)
    {
        if (Enum.TryParse<ZoneId>(token, true, out zone))
        {
            return true;
        }

        if (int.TryParse(token, out var raw) && raw >= (int)ZoneId.OuterFarms && raw <= (int)ZoneId.Keep)
        {
            zone = (ZoneId)raw;
            return true;
        }

        return false;
    }

    private static string Normalize(string value)
    {
        var chars = value.ToLowerInvariant().ToCharArray();
        var buffer = new char[chars.Length];
        var idx = 0;
        foreach (var ch in chars)
        {
            if (char.IsLetterOrDigit(ch))
            {
                buffer[idx++] = ch;
            }
        }

        return new string(buffer, 0, idx);
    }

    private static void ClearAssignments(GameState state, JobAllocation allocation)
    {
        foreach (var job in Enum.GetValues<JobType>())
        {
            allocation.SetWorkers(job, 0);
        }

        allocation.SetIdleWorkers(state.AvailableHealthyWorkersForAllocation);
    }

    private static void FinalizeAllocation(GameState state, JobAllocation allocation)
    {
        var available = state.AvailableHealthyWorkersForAllocation;
        var assigned = allocation.TotalAssigned();
        if (assigned > available)
        {
            throw new InvalidOperationException("Assigned workers exceed available workers.");
        }

        allocation.SetIdleWorkers(available - assigned);
    }

    private static void PrintInvalidAndHelp(ConsoleRenderer renderer, GameState state, string message)
    {
        Console.WriteLine($"Invalid command: {message}");
        renderer.RenderActionReference(state);
    }

    private static JobAllocation BuildStartingAllocation(GameState state, out string? adjustmentMessage)
    {
        var allocation = new JobAllocation();
        foreach (var job in Enum.GetValues<JobType>())
        {
            allocation.SetWorkers(job, state.Allocation.Workers[job]);
        }

        adjustmentMessage = null;
        var available = state.AvailableHealthyWorkersForAllocation;
        if (allocation.TotalAssigned() > available)
        {
            var overflow = allocation.TotalAssigned() - available;
            foreach (var job in Enum.GetValues<JobType>().Reverse())
            {
                if (overflow <= 0)
                {
                    break;
                }

                var assigned = allocation.Workers[job];
                if (assigned <= 0)
                {
                    continue;
                }

                var reduction = Math.Min(assigned, ((overflow + JobAllocation.Step - 1) / JobAllocation.Step) * JobAllocation.Step);
                allocation.SetWorkers(job, assigned - reduction);
                overflow -= reduction;
            }

            adjustmentMessage = "Previous assignments exceeded available workers today and were automatically reduced.";
        }

        allocation.SetIdleWorkers(available - allocation.TotalAssigned());
        return allocation;
    }
}