using System.Text;
using Prot8.Cli;
using Prot8.Jobs;
using Prot8.Laws;
using Prot8.Missions;
using Prot8.Orders;
using Prot8.Resources;
using Prot8.Simulation;

namespace Playtester;

internal static class PromptBuilder
{
    public static string BuildTurnPrompt(GameState state, PendingDayPlan plan, string previousTurnFeedback, int attemptNumber)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"You are controlling Day {state.Day} in the siege game.");
        builder.AppendLine($"This is attempt {attemptNumber} for the same day.");
        builder.AppendLine("Return JSON only with an 'actions' array.");
        builder.AppendLine("Day advances only when you send end_day and no actions are skipped.");
        builder.AppendLine();
        builder.AppendLine("Expected JSON shape:");
        builder.AppendLine("{\"actions\":[{\"type\":\"assign\",\"target\":\"j1\",\"workers\":30},{\"type\":\"enact\",\"target\":\"l1\"},{\"type\":\"end_day\"}],\"reasoning\":\"...\"}");
        builder.AppendLine();

        builder.AppendLine("Previous Attempt Feedback (executed/skipped and outcomes):");
        builder.AppendLine(Truncate(previousTurnFeedback, 4000));
        builder.AppendLine();

        builder.AppendLine("Current Game State:");
        builder.AppendLine($"Day: {state.Day}");
        builder.AppendLine($"Morale: {state.Morale} | Unrest: {state.Unrest} | Sickness: {state.Sickness}");
        builder.AppendLine($"Siege Intensity: {state.SiegeIntensity} | Active Perimeter: {state.ActivePerimeterZone.Name}");
        builder.AppendLine($"Resources -> Food: {state.Resources[ResourceKind.Food]}, Water: {state.Resources[ResourceKind.Water]}, Fuel: {state.Resources[ResourceKind.Fuel]}, Medicine: {state.Resources[ResourceKind.Medicine]}, Materials: {state.Resources[ResourceKind.Materials]}");
        builder.AppendLine($"Population -> Healthy: {state.Population.HealthyWorkers}, Guards: {state.Population.Guards}, Sick: {state.Population.SickWorkers}, Elderly: {state.Population.Elderly}, Total: {state.Population.TotalPopulation}");
        builder.AppendLine($"Workers available for assignment: {state.AvailableHealthyWorkersForAllocation}");
        builder.AppendLine();

        builder.AppendLine("Current Pending Plan For This Day:");
        var jobs = ActionAvailability.GetJobTypes();
        for (var i = 0; i < jobs.Count; i++)
        {
            var job = jobs[i];
            builder.AppendLine($"- j{i + 1} ({job}): {plan.Allocation.Workers[job]} workers");
        }

        builder.AppendLine($"- Idle workers: {plan.Allocation.IdleWorkers}");
        builder.AppendLine($"- Queued optional action: {DescribeQueuedOptionalAction(plan.ActionChoice)}");
        if (plan.Notices.Count > 0)
        {
            builder.AppendLine("- Notices:");
            foreach (var notice in plan.Notices)
            {
                builder.AppendLine($"  - {notice}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("Available Laws:");
        var laws = ActionAvailability.GetAvailableLaws(state);
        if (laws.Count == 0)
        {
            builder.AppendLine("- none");
        }
        else
        {
            for (var i = 0; i < laws.Count; i++)
            {
                builder.AppendLine($"- l{i + 1}: {laws[i].Name} ({laws[i].Id}) -> {laws[i].Summary}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("Available Orders:");
        var orders = ActionAvailability.GetAvailableOrders(state);
        if (orders.Count == 0)
        {
            builder.AppendLine("- none");
        }
        else
        {
            for (var i = 0; i < orders.Count; i++)
            {
                var order = orders[i];
                if (!order.RequiresZoneSelection)
                {
                    builder.AppendLine($"- o{i + 1}: {order.Name} ({order.Id}) -> {order.Summary}");
                }
                else
                {
                    var validZones = ActionAvailability.GetValidZonesForOrder(state, order);
                    var zoneList = validZones.Count == 0 ? "none" : string.Join(", ", validZones);
                    builder.AppendLine($"- o{i + 1}: {order.Name} ({order.Id}) -> {order.Summary} | requires zone from: {zoneList}");
                }
            }
        }

        builder.AppendLine();
        builder.AppendLine("Available Missions:");
        var missions = ActionAvailability.GetAvailableMissions(state);
        if (missions.Count == 0)
        {
            builder.AppendLine("- none");
        }
        else
        {
            for (var i = 0; i < missions.Count; i++)
            {
                builder.AppendLine($"- m{i + 1}: {missions[i].Name} ({missions[i].Id}) -> {missions[i].OutcomeHint}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("Rules reminder:");
        builder.AppendLine("- You can send multiple assign actions.");
        builder.AppendLine("- Only one optional action (law/order/mission) is kept; later one replaces earlier one.");
        builder.AppendLine("- clear_assignments clears all worker assignments.");
        builder.AppendLine("- clear_action clears the queued law/order/mission.");
        builder.AppendLine("- If any action is skipped, end_day is skipped for this attempt.");
        builder.AppendLine("- Use only available references listed above.");
        builder.AppendLine("- End each successful attempt with {\"type\":\"end_day\"}.");

        return builder.ToString();
    }

    public static string BuildAttemptFeedback(GameState state, PendingDayPlan plan, TurnExecutionResult execution, string aiRawResponse, int attemptNumber)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Day {state.Day} attempt {attemptNumber} feedback:");
        builder.AppendLine("Raw AI response:");
        builder.AppendLine(Truncate(aiRawResponse, 2000));
        builder.AppendLine();

        AppendActionResults(builder, execution);

        builder.AppendLine($"end_day requested: {(execution.EndDayRequested ? "yes" : "no")}");
        builder.AppendLine($"end_day accepted: {(execution.EndDayAccepted ? "yes" : "no")}");

        builder.AppendLine();
        builder.AppendLine("Pending day plan after this attempt:");
        var jobs = ActionAvailability.GetJobTypes();
        for (var i = 0; i < jobs.Count; i++)
        {
            var job = jobs[i];
            builder.AppendLine($"- j{i + 1} ({job}): {plan.Allocation.Workers[job]} workers");
        }

        builder.AppendLine($"- Idle workers: {plan.Allocation.IdleWorkers}");
        builder.AppendLine($"- Queued optional action: {DescribeQueuedOptionalAction(plan.ActionChoice)}");

        if (!execution.EndDayAccepted)
        {
            builder.AppendLine();
            builder.AppendLine("Day not resolved yet. Send corrective actions and include end_day again.");
        }

        return builder.ToString();
    }

    public static string BuildTurnFeedback(GameState state, DayResolutionReport report, TurnExecutionResult execution, string aiRawResponse)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Turn {report.Day} feedback:");
        builder.AppendLine("Raw AI response:");
        builder.AppendLine(Truncate(aiRawResponse, 2000));
        builder.AppendLine();

        AppendActionResults(builder, execution);
        builder.AppendLine($"end_day requested: {(execution.EndDayRequested ? "yes" : "no")}");
        builder.AppendLine($"end_day accepted: {(execution.EndDayAccepted ? "yes" : "no")}");

        builder.AppendLine();
        builder.AppendLine("Day result highlights:");
        builder.AppendLine($"- Food: {state.Resources[ResourceKind.Food]}, Water: {state.Resources[ResourceKind.Water]}, Fuel: {state.Resources[ResourceKind.Fuel]}, Medicine: {state.Resources[ResourceKind.Medicine]}, Materials: {state.Resources[ResourceKind.Materials]}");
        builder.AppendLine($"- Morale: {state.Morale}, Unrest: {state.Unrest}, Sickness: {state.Sickness}, SiegeIntensity: {state.SiegeIntensity}");
        builder.AppendLine($"- Active perimeter: {state.ActivePerimeterZone.Name}");
        builder.AppendLine($"- Events triggered: {(report.TriggeredEvents.Count == 0 ? "none" : string.Join(", ", report.TriggeredEvents))}");
        builder.AppendLine($"- Missions resolved: {(report.ResolvedMissions.Count == 0 ? "none" : string.Join(" | ", report.ResolvedMissions))}");

        if (!string.IsNullOrWhiteSpace(state.GameOverDetails))
        {
            builder.AppendLine($"- GameOverDetails: {state.GameOverDetails}");
        }

        return builder.ToString();
    }

    public static string BuildPostmortemPrompt(GameState state, IReadOnlyList<string> turnHistory)
    {
        var builder = new StringBuilder();
        builder.AppendLine("The run is finished. Explain WHY the run resulted in this outcome.");
        builder.AppendLine();
        builder.AppendLine($"Outcome: {(state.Survived ? "Survived Day 40" : state.GameOverCause.ToString())}");
        builder.AppendLine($"Final Day: {state.Day}");
        builder.AppendLine($"Final Stats -> Morale: {state.Morale}, Unrest: {state.Unrest}, Sickness: {state.Sickness}");
        builder.AppendLine($"Final Resources -> Food: {state.Resources[ResourceKind.Food]}, Water: {state.Resources[ResourceKind.Water]}, Fuel: {state.Resources[ResourceKind.Fuel]}, Medicine: {state.Resources[ResourceKind.Medicine]}, Materials: {state.Resources[ResourceKind.Materials]}");
        builder.AppendLine();
        builder.AppendLine("Recent turn feedback for context:");

        var startIndex = Math.Max(0, turnHistory.Count - 5);
        for (var i = startIndex; i < turnHistory.Count; i++)
        {
            builder.AppendLine($"--- Turn Context {i + 1} ---");
            builder.AppendLine(Truncate(turnHistory[i], 2500));
        }

        builder.AppendLine();
        builder.AppendLine("Respond in concise markdown with:");
        builder.AppendLine("1. Main cause of win/loss");
        builder.AppendLine("2. Key decision mistakes or strengths");
        builder.AppendLine("3. One concrete improvement for next run");

        return builder.ToString();
    }

    private static void AppendActionResults(StringBuilder builder, TurnExecutionResult execution)
    {
        builder.AppendLine("Executed actions:");
        if (execution.Executed.Count == 0)
        {
            builder.AppendLine("- none");
        }
        else
        {
            foreach (var line in execution.Executed)
            {
                builder.AppendLine($"- {line}");
            }
        }

        builder.AppendLine("Skipped actions:");
        if (execution.Skipped.Count == 0)
        {
            builder.AppendLine("- none");
        }
        else
        {
            foreach (var line in execution.Skipped)
            {
                builder.AppendLine($"- {line}");
            }
        }
    }

    private static string DescribeQueuedOptionalAction(TurnActionChoice actionChoice)
    {
        if (!actionChoice.HasAction)
        {
            return "none";
        }

        if (!string.IsNullOrWhiteSpace(actionChoice.LawId))
        {
            var law = LawCatalog.Find(actionChoice.LawId);
            return $"law: {law?.Name ?? actionChoice.LawId}";
        }

        if (!string.IsNullOrWhiteSpace(actionChoice.EmergencyOrderId))
        {
            var order = EmergencyOrderCatalog.Find(actionChoice.EmergencyOrderId);
            var zoneSuffix = actionChoice.SelectedZoneForOrder.HasValue ? $" ({actionChoice.SelectedZoneForOrder.Value})" : string.Empty;
            return $"order: {(order?.Name ?? actionChoice.EmergencyOrderId)}{zoneSuffix}";
        }

        if (string.IsNullOrWhiteSpace(actionChoice.MissionId))
        {
            return "none";
        }

        var mission = MissionCatalog.Find(actionChoice.MissionId);
        return $"mission: {mission?.Name ?? actionChoice.MissionId}";
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value.Substring(value.Length - maxLength);
    }
}
