using System.Text;
using Prot8.Resources;
using Prot8.Simulation;

namespace Playtester;

internal static class PromptBuilder
{
    public static string BuildPostmortemSystemPrompt()
    {
        var builder = new StringBuilder();
        builder.AppendLine("You are evaluating the completed run of a siege survival simulation.");
        builder.AppendLine("Write clear, concise markdown analysis only.");
        builder.AppendLine("Do not output JSON.");
        builder.AppendLine("Do not include code fences unless needed.");
        return builder.ToString();
    }

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

        builder.AppendLine("CLI Game State Snapshot (renderer output):");
        builder.AppendLine("```text");
        builder.AppendLine(Truncate(ConsoleRenderCapture.RenderDayStart(state), 14000));
        builder.AppendLine("```");
        builder.AppendLine();

        builder.AppendLine("CLI Pending Day Plan Snapshot (renderer output):");
        builder.AppendLine("```text");
        builder.AppendLine(Truncate(ConsoleRenderCapture.RenderPendingPlan(state, plan.Allocation, plan.ActionChoice), 6000));
        builder.AppendLine("```");
        if (plan.Notices.Count > 0)
        {
            builder.AppendLine("Plan Notices:");
            foreach (var notice in plan.Notices)
            {
                builder.AppendLine($"- {notice}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("Rules reminder:");
        builder.AppendLine("- You can send multiple assign actions.");
        builder.AppendLine("- Only one optional action (law/order/mission) is kept; later one replaces earlier one.");
        builder.AppendLine("- clear_assignments clears all worker assignments.");
        builder.AppendLine("- clear_action clears the queued law/order/mission.");
        builder.AppendLine("- If any action is skipped, end_day is skipped for this attempt.");
        builder.AppendLine("- Use only references listed in the renderer snapshot.");
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
        builder.AppendLine("Pending day plan after this attempt (renderer output):");
        builder.AppendLine("```text");
        builder.AppendLine(Truncate(ConsoleRenderCapture.RenderPendingPlan(state, plan.Allocation, plan.ActionChoice), 6000));
        builder.AppendLine("```");

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
        builder.AppendLine("Resolved day report (renderer output):");
        builder.AppendLine("```text");
        builder.AppendLine(Truncate(ConsoleRenderCapture.RenderDayReport(state, report), 9000));
        builder.AppendLine("```");

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

        var startIndex = Math.Max(0, turnHistory.Count - 3);
        for (var i = startIndex; i < turnHistory.Count; i++)
        {
            builder.AppendLine($"--- Turn Context {i + 1} ---");
            builder.AppendLine(Truncate(turnHistory[i], 1400));
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

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value.Substring(value.Length - maxLength);
    }
}
