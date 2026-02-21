using System;
using System.Linq;
using Prot8.Cli.Output;
using Prot8.Constants;
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
            message = "Usage: assign <JobType> <Workers>.";
            return false;
        }

        if (!Enum.TryParse<JobType>(parts[1], true, out var job))
        {
            message = $"Unknown JobType '{parts[1]}'.";
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
            message = "Usage: enact_law <LawId>.";
            return false;
        }

        var law = ResolveLaw(parts[1]);
        if (law is null)
        {
            message = $"Unknown law '{parts[1]}'.";
            return false;
        }

        if (state.ActiveLawIds.Contains(law.Id))
        {
            message = $"Law already enacted: {law.Name}.";
            return false;
        }

        var lawCooldownActive = state.LastLawDay != int.MinValue
            && state.Day - state.LastLawDay < GameBalance.LawCooldownDays;
        if (lawCooldownActive)
        {
            message = $"Law cooldown active. Next law day: {state.LastLawDay + GameBalance.LawCooldownDays}.";
            return false;
        }

        if (!law.CanEnact(state, out var reason))
        {
            message = $"Cannot enact {law.Name}: {reason}";
            return false;
        }

        action = new TurnActionChoice { LawId = law.Id };
        message = $"Queued law for today: {law.Name}.";
        return true;
    }

    private static bool TryQueueOrder(GameState state, ref TurnActionChoice action, string[] parts, out string message)
    {
        if (parts.Length < 2 || parts.Length > 3)
        {
            message = "Usage: issue_order <OrderId> [ZoneId].";
            return false;
        }

        var order = ResolveOrder(parts[1]);
        if (order is null)
        {
            message = $"Unknown order '{parts[1]}'.";
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

        if (order.RequiresZoneSelection && !zone.HasValue)
        {
            message = $"{order.Name} requires a ZoneId parameter.";
            return false;
        }

        if (!order.RequiresZoneSelection && zone.HasValue)
        {
            message = $"{order.Name} does not accept a ZoneId parameter.";
            return false;
        }

        if (!order.CanIssue(state, zone, out var reason))
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
            message = "Usage: start_mission <MissionId>.";
            return false;
        }

        var mission = ResolveMission(parts[1]);
        if (mission is null)
        {
            message = $"Unknown mission '{parts[1]}'.";
            return false;
        }

        if (!mission.CanStart(state, out var reason))
        {
            message = $"Cannot start mission {mission.Name}: {reason}";
            return false;
        }

        action = new TurnActionChoice { MissionId = mission.Id };
        message = $"Queued mission for today: {mission.Name}.";
        return true;
    }

    private static ILaw? ResolveLaw(string token)
    {
        var normalized = Normalize(token);
        foreach (var law in LawCatalog.GetAll())
        {
            if (string.Equals(law.Id, token, StringComparison.OrdinalIgnoreCase))
            {
                return law;
            }

            if (Normalize(law.Name) == normalized)
            {
                return law;
            }
        }

        return null;
    }

    private static IEmergencyOrder? ResolveOrder(string token)
    {
        var normalized = Normalize(token);
        foreach (var order in EmergencyOrderCatalog.GetAll())
        {
            if (string.Equals(order.Id, token, StringComparison.OrdinalIgnoreCase))
            {
                return order;
            }

            if (Normalize(order.Name) == normalized)
            {
                return order;
            }
        }

        return null;
    }

    private static IMissionDefinition? ResolveMission(string token)
    {
        var normalized = Normalize(token);
        foreach (var mission in MissionCatalog.GetAll())
        {
            if (string.Equals(mission.Id, token, StringComparison.OrdinalIgnoreCase))
            {
                return mission;
            }

            if (Normalize(mission.Name) == normalized)
            {
                return mission;
            }
        }

        return null;
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
