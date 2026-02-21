using Prot8.Cli.Input;
using Prot8.Simulation;

namespace Playtester;

internal static class AiTurnExecutor
{
    public static PendingDayPlan BuildInitialPlan(GameState state)
    {
        var notices = new List<string>();
        var allocation = ConsoleInputReader.BuildStartingAllocation(state, out var adjustmentMessage);
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

            if (!TryBuildCommand(action, out var commandText, out var commandReason))
            {
                skipped.Add($"{label} skipped: {commandReason}");
                continue;
            }

            if (ConsoleInputReader.TryExecuteCommand(state, plan.Allocation, ref queuedChoice, commandText!, out var executionMessage, out var requestedEndDay))
            {
                executed.Add($"{label} {executionMessage}");
                if (requestedEndDay)
                {
                    endDayRequested = true;
                }
            }
            else
            {
                skipped.Add($"{label} skipped: {executionMessage}");
            }
        }

        plan.ActionChoice = queuedChoice;
        ConsoleInputReader.FinalizeAllocation(state, plan.Allocation);

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

    private static bool TryBuildCommand(AiPlannedAction action, out string? commandText, out string reason)
    {
        commandText = null;
        if (string.IsNullOrWhiteSpace(action.Type))
        {
            reason = "missing action type.";
            return false;
        }

        var type = action.Type.Trim().ToLowerInvariant();
        switch (type)
        {
            case "assign":
                if (string.IsNullOrWhiteSpace(action.Target))
                {
                    reason = "assign requires target job (for example: j1).";
                    return false;
                }

                if (!action.Workers.HasValue)
                {
                    reason = "assign requires workers.";
                    return false;
                }

                commandText = $"assign {action.Target} {action.Workers.Value}";
                reason = string.Empty;
                return true;

            case "enact":
            case "enact_law":
                if (string.IsNullOrWhiteSpace(action.Target))
                {
                    reason = "enact requires target law (for example: l1).";
                    return false;
                }

                commandText = $"enact {action.Target}";
                reason = string.Empty;
                return true;

            case "order":
            case "issue_order":
                if (string.IsNullOrWhiteSpace(action.Target))
                {
                    reason = "order requires target order (for example: o1).";
                    return false;
                }

                commandText = string.IsNullOrWhiteSpace(action.Zone)
                    ? $"order {action.Target}"
                    : $"order {action.Target} {action.Zone}";
                reason = string.Empty;
                return true;

            case "mission":
            case "start_mission":
                if (string.IsNullOrWhiteSpace(action.Target))
                {
                    reason = "mission requires target mission (for example: m1).";
                    return false;
                }

                commandText = $"mission {action.Target}";
                reason = string.Empty;
                return true;

            case "clear_assignments":
                commandText = "clear_assignments";
                reason = string.Empty;
                return true;

            case "clear_action":
                commandText = "clear_action";
                reason = string.Empty;
                return true;

            case "end_day":
                commandText = "end_day";
                reason = string.Empty;
                return true;

            default:
                reason = $"unknown action type '{action.Type}'.";
                return false;
        }
    }
}
