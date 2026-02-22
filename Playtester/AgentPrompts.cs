using System.Text.Json.Nodes;

namespace Playtester;

public static class AgentPrompts
{
    // ── Commander ────────────────────────────────────────────────────────────

    public const string CommanderSystem = """
        You are playing a siege survival city manager through a CLI interface.

        Your objective is to survive until Day 40.

        Rules:
        - You may only use commands explicitly listed in the snapshot.
        - Total assigned workers must not exceed the available workers shown.
        - Each day, You may select to queue ONLY one of Emergency Order, Law or Mission. 
        - Do not invent mechanics, rules, commands, or hidden information.
        - Respond with a JSON object. The "commands" field is an ordered array of objects,
          each with a "command" string (the CLI command to run) and a "reason" string
          (one sentence explaining why). The last entry must have command="end_day".
        """;

    public static string CommanderUser(string daySnapshot, string notebook, string previousRunLearnings, string? validationErrors = null)
    {
        if (!string.IsNullOrEmpty(previousRunLearnings))
        {
            previousRunLearnings = "<prevoius_learnings>\n" + previousRunLearnings + "</prevoius_learnings>";
        }
        
        var errorSection = validationErrors is null ? "" : $"""
            VALIDATION ERRORS FROM YOUR PREVIOUS ATTEMPT
            The following commands were rejected. You must fix all of them before the day can proceed.
            ```
            {validationErrors}
            ```

            """;

        return $"""
            {errorSection}
            <day_snapshot>
            {daySnapshot}
            </day_snapshot>

            <notebook>
            {notebook}
            </notebook>

            {previousRunLearnings}

            Win and Loss Conditions
            - Win: reach Day 40.
            - Lose: Keep integrity reaches 0 or below.
            - Lose: Unrest rises above 85.
            - Lose: Food and Water are both 0 for 2 consecutive days.
            
            Respond with a JSON object. The "commands" field is an ordered array of CLI command strings to execute today. 
            The last command must be "end_day".
            """;
    }

    public static readonly JsonNode CommanderResponseFormat = new JsonObject
    {
        ["type"] = "json_schema",
        ["json_schema"] = new JsonObject
        {
            ["name"] = "commander_response",
            ["strict"] = true,
            ["schema"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["commands"] = new JsonObject
                    {
                        ["type"] = "array",
                        ["description"] = "Ordered commands to execute today. Last entry must have command=\"end_day\".",
                        ["items"] = new JsonObject
                        {
                            ["type"] = "object",
                            ["properties"] = new JsonObject
                            {
                                ["command"] = new JsonObject { ["type"] = "string", ["description"] = "The CLI command string to execute." },
                                ["reason"] = new JsonObject { ["type"] = "string", ["description"] = "One-sentence reason for issuing this command." }
                            },
                            ["required"] = new JsonArray("command", "reason"),
                            ["additionalProperties"] = false
                        }
                    }
                },
                ["required"] = new JsonArray("commands"),
                ["additionalProperties"] = false
            }
        }
    };

    // ── Scribe ───────────────────────────────────────────────────────────────

    public const string ScribeSystem = """
        You are the evolving notebook of a playtester.

        Your job is to update the player's beliefs based strictly on:
        - The start-of-day snapshot
        - The commands executed
        - The resolution log
        
        Try to validate your hypotheses against the resolution log. 
        If a hypothesis is contradicted by the resolution log, discard it. 
        If it is supported by the resolution log, move it to observations.
        
        Rules:
        - Do not invent mechanics or hidden systems.
        - Do not assume knowledge not shown in logs.
        - Record trends and patterns only if supported by evidence.
        - Respond with a JSON object with four fields: hypotheses, observations,
          open_questions, plan. Each is an array of short strings.
        """;

    public static string ScribeUser(string previousNotebook, string daySnapshot, string commandsExecuted, string resolutionLog, string previousRunLearnings)
    {
        if (!string.IsNullOrEmpty(previousRunLearnings))
        {
            previousRunLearnings = "<previous_learnings>\n" + previousRunLearnings + "\n</previous_learnings>";
        }
        
        return $"""
            <old_notebook>
            {previousNotebook}
            </old_notebook>

            <current_day>
            {daySnapshot}
            </current_day>

            <commands>
            {commandsExecuted}
            </commands>

            <resolution>
            {resolutionLog}
            </resolution>

            {previousRunLearnings}

            Update the notebook. Return JSON with fields:
            hypotheses, observations, open_questions, plan.
            Each field is an array of concise strings.
            """;
    }

    public static readonly JsonNode ScribeResponseFormat = new JsonObject
    {
        ["type"] = "json_schema",
        ["json_schema"] = new JsonObject
        {
            ["name"] = "scribe_notebook",
            ["strict"] = true,
            ["schema"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["hypotheses"] = StringArray("Current beliefs about how the game works."),
                    ["observations"] = StringArray("Concrete things noticed this day."),
                    ["open_questions"] = StringArray("Things still unknown or uncertain."),
                    ["plan"] = StringArray("Intended strategy for upcoming days.")
                },
                ["required"] = new JsonArray("hypotheses", "observations", "open_questions", "plan"),
                ["additionalProperties"] = false
            }
        }
    };

    // ── Critic ───────────────────────────────────────────────────────────────

    public const string CriticSystem = """
        You are a playtester writing a postmortem after completing a siege survival run.

        You must ground all conclusions in:
        - The final state summary
        - The day-by-day timeline (each entry includes the full state snapshot, commands, and resolution signals)
        - The playtester notebook

        Rules:
        - Do not invent mechanics not shown in the logs.
        - Do not assume hidden systems.
        - Be specific and concrete.
        - Distinguish between what felt unclear and what was clearly communicated.
        - Do not write as a designer. Write as a player reflecting on the experience.
        - Keep total postmortem under 900 words.
        - Generate exactly 10 actionable learnings the commander should apply in the next run.
        - If learnings from a previous run are provided, evaluate whether the commander demonstrably improved this run.
        - Respond with a JSON object using the fields defined in the schema.
        """;

    public static string CriticUser(string finalSummary, string timeline, string finalNotebook, string previousRunLearnings)
    {
        return $"""
            FINAL SUMMARY
            <<<
            {finalSummary}
            >>>

            TIMELINE
            Format: Each day block shows the full state snapshot, commands executed, and key resolution signals.
            <<<
            {timeline}
            >>>

            FINAL NOTEBOOK
            <<<
            {finalNotebook}
            >>>

            LEARNINGS FROM PREVIOUS RUN
            <<<
            {previousRunLearnings}
            >>>

            Write the postmortem covering: outcome, cause, three impactful decisions,
            strategy, what felt unclear, what felt fair vs unfair, suggestions,
            what you would try next run, exactly 10 actionable learnings for the next run,
            and whether the commander did a better job this run compared to the previous run
            (use the previous run learnings as your benchmark — if this is the first run, say so).
            """;
    }

    public static readonly JsonNode CriticResponseFormat = new JsonObject
    {
        ["type"] = "json_schema",
        ["json_schema"] = new JsonObject
        {
            ["name"] = "critic_postmortem",
            ["strict"] = true,
            ["schema"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["outcome"] = new JsonObject { ["type"] = "string", ["description"] = "One-sentence summary of how the run ended." },
                    ["cause"] = new JsonObject { ["type"] = "string", ["description"] = "What the player believes caused the outcome." },
                    ["impactful_decisions"] = new JsonObject
                    {
                        ["type"] = "array",
                        ["description"] = "Exactly three decisions that most affected the run.",
                        ["items"] = new JsonObject { ["type"] = "string" },
                        ["minItems"] = 3,
                        ["maxItems"] = 3
                    },
                    ["strategy"] = new JsonObject { ["type"] = "string", ["description"] = "The dominant strategy the player converged on, if any." },
                    ["unclear"] = new JsonObject { ["type"] = "string", ["description"] = "What felt unclear or confusing during the run." },
                    ["fair_vs_unfair"] = new JsonObject { ["type"] = "string", ["description"] = "What felt fair and what felt unfair." },
                    ["suggestions"] = new JsonObject { ["type"] = "string", ["description"] = "Concrete changes the player would suggest." },
                    ["next_run"] = new JsonObject { ["type"] = "string", ["description"] = "What the player would try differently next run." },
                    ["learnings"] = new JsonObject
                    {
                        ["type"] = "array",
                        ["description"] = "Exactly 10 actionable learnings for the commander to apply in the next run.",
                        ["items"] = new JsonObject { ["type"] = "string" },
                        ["minItems"] = 10,
                        ["maxItems"] = 10
                    },
                    ["better_than_previous"] = new JsonObject { ["type"] = "string", ["description"] = "Assessment of whether the commander performed better than the previous run, with specific reasoning." }
                },
                ["required"] = new JsonArray("outcome", "cause", "impactful_decisions", "strategy", "unclear", "fair_vs_unfair", "suggestions", "next_run", "learnings", "better_than_previous"),
                ["additionalProperties"] = false
            }
        }
    };

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static JsonObject StringArray(string description) => new()
    {
        ["type"] = "array",
        ["description"] = description,
        ["items"] = new JsonObject { ["type"] = "string" }
    };
}
