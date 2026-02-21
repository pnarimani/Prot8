using System.Text;
using Prot8.Cli;
using Prot8.Resources;
using Prot8.Simulation;

namespace Playtester;

internal static class PromptBuilder
{
    public static string BuildTurnPrompt(GameState state, string previousTurnFeedback)
    {
        var builder = new StringBuilder();
        builder.AppendLine("You are controlling one day in the siege game.");
        builder.AppendLine("Return JSON only with an 'actions' array.");
        builder.AppendLine("Use only listed available options.");
        builder.AppendLine();
        builder.AppendLine("Expected JSON shape:");
        builder.AppendLine("{\"actions\":[{\"type\":\"assign\",\"target\":\"j1\",\"workers\":30},{\"type\":\"enact\",\"target\":\"l1\"}],\"reasoning\":\"...\"}");
        builder.AppendLine();

        builder.AppendLine("Previous Turn Feedback (executed/skipped and outcomes):");
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

        builder.AppendLine("Current Assignments:");
        var jobs = ActionAvailability.GetJobTypes();
        for (var i = 0; i < jobs.Count; i++)
        {
            var job = jobs[i];
            builder.AppendLine($"- j{i + 1} ({job}): {state.Allocation.Workers[job]} workers");
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
        builder.AppendLine("- Only one optional action (law/order/mission) effectively applies each day; later queued action replaces earlier one.");
        builder.AppendLine("- Invalid actions are skipped.");
        builder.AppendLine("- Use only available references listed above.");

        return builder.ToString();
    }

    public static string BuildTurnFeedback(GameState state, DayResolutionReport report, TurnExecutionResult execution, string aiRawResponse)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Turn {report.Day} feedback:");
        builder.AppendLine("Raw AI response:");
        builder.AppendLine(Truncate(aiRawResponse, 2000));
        builder.AppendLine();

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

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value.Substring(value.Length - maxLength);
    }
}
