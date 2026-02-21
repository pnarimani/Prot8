using Prot8.Cli;
using Prot8.Jobs;
using Prot8.Laws;
using Prot8.Missions;
using Prot8.Orders;
using Prot8.Simulation;
using Prot8.Zones;

namespace Playtester;

internal static class AiTurnExecutor
{
    public static PendingDayPlan BuildInitialPlan(GameState state)
    {
        var notices = new List<string>();
        var allocation = BuildStartingAllocation(state, out var adjustmentMessage);
        if (!string.IsNullOrWhiteSpace(adjustmentMessage))
        {
            notices.Add(adjustmentMessage);
        }

        return new PendingDayPlan(allocation, new TurnActionChoice(), notices);
    }

    public static TurnExecutionResult Execute(GameState state, PendingDayPlan plan, IReadOnlyList<AiPlannedAction> actions, string? parseWarning)
    {
        var executed = new List<string>();
        var skipped = new List<string>();
        var endDayRequested = false;
        var queuedChoice = plan.ActionChoice;

        if (!string.IsNullOrWhiteSpace(parseWarning))
        {
            skipped.Add(parseWarning);
        }

        for (var index = 0; index < actions.Count; index++)
        {
            var action = actions[index];
            var label = $"#{index + 1}";

            if (string.IsNullOrWhiteSpace(action.Type))
            {
                skipped.Add($"{label} skipped: missing action type.");
                continue;
            }

            var type = action.Type.Trim().ToLowerInvariant();
            switch (type)
            {
                case "assign":
                    if (TryApplyAssign(state, plan.Allocation, action, out var assignMessage))
                    {
                        executed.Add($"{label} {assignMessage}");
                    }
                    else
                    {
                        skipped.Add($"{label} skipped: {assignMessage}");
                    }

                    break;

                case "enact":
                case "enact_law":
                    if (TryQueueLaw(state, ref queuedChoice, action, out var lawMessage))
                    {
                        executed.Add($"{label} {lawMessage}");
                    }
                    else
                    {
                        skipped.Add($"{label} skipped: {lawMessage}");
                    }

                    break;

                case "order":
                case "issue_order":
                    if (TryQueueOrder(state, ref queuedChoice, action, out var orderMessage))
                    {
                        executed.Add($"{label} {orderMessage}");
                    }
                    else
                    {
                        skipped.Add($"{label} skipped: {orderMessage}");
                    }

                    break;

                case "mission":
                case "start_mission":
                    if (TryQueueMission(state, ref queuedChoice, action, out var missionMessage))
                    {
                        executed.Add($"{label} {missionMessage}");
                    }
                    else
                    {
                        skipped.Add($"{label} skipped: {missionMessage}");
                    }

                    break;

                case "clear_assignments":
                    ClearAssignments(state, plan.Allocation);
                    executed.Add($"{label} assignments cleared.");
                    break;

                case "clear_action":
                    queuedChoice = new TurnActionChoice();
                    executed.Add($"{label} queued day action cleared.");
                    break;

                case "end_day":
                    endDayRequested = true;
                    executed.Add($"{label} end_day requested.");
                    break;

                default:
                    skipped.Add($"{label} skipped: unknown action type '{action.Type}'.");
                    break;
            }
        }

        plan.ActionChoice = queuedChoice;
        FinalizeAllocation(state, plan.Allocation);

        var endDayAccepted = false;
        if (endDayRequested)
        {
            if (skipped.Count == 0)
            {
                endDayAccepted = true;
                executed.Add("end_day accepted.");
            }
            else
            {
                skipped.Add("end_day skipped: one or more actions were skipped this turn; send corrective actions, then send end_day again.");
            }
        }

        return new TurnExecutionResult(executed, skipped, endDayRequested, endDayAccepted);
    }

    private static bool TryApplyAssign(GameState state, JobAllocation allocation, AiPlannedAction action, out string message)
    {
        if (string.IsNullOrWhiteSpace(action.Target))
        {
            message = "assign requires target job (for example: j1).";
            return false;
        }

        if (!TryResolveJob(action.Target, out var job, out message))
        {
            return false;
        }

        if (!action.Workers.HasValue)
        {
            message = "assign requires workers.";
            return false;
        }

        var workers = action.Workers.Value;
        if (workers < 0)
        {
            message = "workers cannot be negative.";
            return false;
        }

        if (workers % JobAllocation.Step != 0)
        {
            message = $"workers must be in increments of {JobAllocation.Step}.";
            return false;
        }

        var available = state.AvailableHealthyWorkersForAllocation;
        var currentForJob = allocation.Workers[job];
        var newTotal = allocation.TotalAssigned() - currentForJob + workers;
        if (newTotal > available)
        {
            message = $"assignment exceeds available workers ({newTotal}/{available}).";
            return false;
        }

        allocation.SetWorkers(job, workers);
        allocation.SetIdleWorkers(available - newTotal);

        message = $"assign {job}={workers} accepted.";
        return true;
    }

    private static bool TryQueueLaw(GameState state, ref TurnActionChoice choice, AiPlannedAction action, out string message)
    {
        if (string.IsNullOrWhiteSpace(action.Target))
        {
            message = "enact requires target law (for example: l1).";
            return false;
        }

        if (!TryResolveLaw(state, action.Target, out var law, out message))
        {
            return false;
        }

        choice = new TurnActionChoice { LawId = law!.Id };
        message = $"enact {law.Name} queued.";
        return true;
    }

    private static bool TryQueueOrder(GameState state, ref TurnActionChoice choice, AiPlannedAction action, out string message)
    {
        if (string.IsNullOrWhiteSpace(action.Target))
        {
            message = "order requires target order (for example: o1).";
            return false;
        }

        if (!TryResolveOrder(state, action.Target, out var order, out message))
        {
            return false;
        }

        ZoneId? zone = null;
        if (order!.RequiresZoneSelection)
        {
            if (string.IsNullOrWhiteSpace(action.Zone))
            {
                message = $"order {order.Name} requires zone.";
                return false;
            }

            if (!TryParseZone(action.Zone, out var parsedZone))
            {
                message = $"unknown zone '{action.Zone}'.";
                return false;
            }

            zone = parsedZone;
        }

        if (!order.CanIssue(state, zone, out var reason))
        {
            message = $"cannot issue {order.Name}: {reason}";
            return false;
        }

        choice = new TurnActionChoice
        {
            EmergencyOrderId = order.Id,
            SelectedZoneForOrder = zone
        };

        message = zone.HasValue ? $"order {order.Name}({zone.Value}) queued." : $"order {order.Name} queued.";
        return true;
    }

    private static bool TryQueueMission(GameState state, ref TurnActionChoice choice, AiPlannedAction action, out string message)
    {
        if (string.IsNullOrWhiteSpace(action.Target))
        {
            message = "mission requires target mission (for example: m1).";
            return false;
        }

        if (!TryResolveMission(state, action.Target, out var mission, out message))
        {
            return false;
        }

        choice = new TurnActionChoice { MissionId = mission!.Id };
        message = $"mission {mission.Name} queued.";
        return true;
    }

    private static bool TryResolveJob(string token, out JobType job, out string reason)
    {
        if (Enum.TryParse<JobType>(token, true, out job))
        {
            reason = string.Empty;
            return true;
        }

        var jobs = ActionAvailability.GetJobTypes();
        if (TryParseShortcut(token, 'j', out var shortcut) && shortcut >= 1 && shortcut <= jobs.Count)
        {
            job = jobs[shortcut - 1];
            reason = string.Empty;
            return true;
        }

        reason = $"unknown job '{token}'.";
        return false;
    }

    private static bool TryResolveLaw(GameState state, string token, out ILaw? law, out string reason)
    {
        law = null;
        var available = ActionAvailability.GetAvailableLaws(state);

        if (TryParseShortcut(token, 'l', out var shortcut))
        {
            if (shortcut >= 1 && shortcut <= available.Count)
            {
                law = available[shortcut - 1];
                reason = string.Empty;
                return true;
            }

            reason = $"law shortcut '{token}' is out of range.";
            return false;
        }

        foreach (var item in available)
        {
            if (string.Equals(item.Id, token, StringComparison.OrdinalIgnoreCase)
                || string.Equals(Normalize(item.Name), Normalize(token), StringComparison.Ordinal))
            {
                law = item;
                reason = string.Empty;
                return true;
            }
        }

        reason = $"law '{token}' is not available.";
        return false;
    }

    private static bool TryResolveOrder(GameState state, string token, out IEmergencyOrder? order, out string reason)
    {
        order = null;
        var available = ActionAvailability.GetAvailableOrders(state);

        if (TryParseShortcut(token, 'o', out var shortcut))
        {
            if (shortcut >= 1 && shortcut <= available.Count)
            {
                order = available[shortcut - 1];
                reason = string.Empty;
                return true;
            }

            reason = $"order shortcut '{token}' is out of range.";
            return false;
        }

        foreach (var item in available)
        {
            if (string.Equals(item.Id, token, StringComparison.OrdinalIgnoreCase)
                || string.Equals(Normalize(item.Name), Normalize(token), StringComparison.Ordinal))
            {
                order = item;
                reason = string.Empty;
                return true;
            }
        }

        reason = $"order '{token}' is not available.";
        return false;
    }

    private static bool TryResolveMission(GameState state, string token, out IMissionDefinition? mission, out string reason)
    {
        mission = null;
        var available = ActionAvailability.GetAvailableMissions(state);

        if (TryParseShortcut(token, 'm', out var shortcut))
        {
            if (shortcut >= 1 && shortcut <= available.Count)
            {
                mission = available[shortcut - 1];
                reason = string.Empty;
                return true;
            }

            reason = $"mission shortcut '{token}' is out of range.";
            return false;
        }

        foreach (var item in available)
        {
            if (string.Equals(item.Id, token, StringComparison.OrdinalIgnoreCase)
                || string.Equals(Normalize(item.Name), Normalize(token), StringComparison.Ordinal))
            {
                mission = item;
                reason = string.Empty;
                return true;
            }
        }

        reason = $"mission '{token}' is not available.";
        return false;
    }

    private static bool TryParseShortcut(string token, char prefix, out int number)
    {
        number = 0;
        if (string.IsNullOrWhiteSpace(token) || token.Length < 2 || char.ToLowerInvariant(token[0]) != char.ToLowerInvariant(prefix))
        {
            return false;
        }

        return int.TryParse(token.Substring(1), out number);
    }

    private static string Normalize(string value)
    {
        var chars = value.ToLowerInvariant().ToCharArray();
        var buffer = new char[chars.Length];
        var index = 0;
        foreach (var ch in chars)
        {
            if (char.IsLetterOrDigit(ch))
            {
                buffer[index++] = ch;
            }
        }

        return new string(buffer, 0, index);
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

            adjustmentMessage = "Persisted assignments were reduced because available workers dropped.";
        }

        allocation.SetIdleWorkers(available - allocation.TotalAssigned());
        return allocation;
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

    private static void ClearAssignments(GameState state, JobAllocation allocation)
    {
        foreach (var job in Enum.GetValues<JobType>())
        {
            allocation.SetWorkers(job, 0);
        }

        allocation.SetIdleWorkers(state.AvailableHealthyWorkersForAllocation);
    }
}
