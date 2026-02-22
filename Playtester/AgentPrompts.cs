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

    public static string CommanderUser(string daySnapshot, string notebook)
    {
        return $"""
            DAY SNAPSHOT
            <<<
            {daySnapshot}
            >>>

            PLAYTESTER NOTEBOOK
            <<<
            {notebook}
            >>>
            
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

        Rules:
        - Do not invent mechanics or hidden systems.
        - Do not assume knowledge not shown in logs.
        - Record trends and patterns only if supported by evidence.
        - Respond with a JSON object with four fields: hypotheses, observations,
          open_questions, plan. Each is an array of short strings.
        - Keep total content concise (aim for under 1200 characters across all fields).
        """;

    public static string ScribeUser(string previousNotebook, string daySnapshot, string commandsExecuted, string resolutionLog)
    {
        return $"""
            OLD NOTEBOOK
            <<<
            {previousNotebook}
            >>>

            START OF DAY SNAPSHOT
            <<<
            {daySnapshot}
            >>>

            COMMANDS EXECUTED
            <<<
            {commandsExecuted}
            >>>

            RESOLUTION LOG
            <<<
            {resolutionLog}
            >>>

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
        - The day-by-day timeline
        - The playtester notebook

        Rules:
        - Do not invent mechanics not shown in the logs.
        - Do not assume hidden systems.
        - Be specific and concrete.
        - Distinguish between what felt unclear and what was clearly communicated.
        - Do not write as a designer. Write as a player reflecting on the experience.
        - Keep total postmortem under 900 words.
        - Respond with a JSON object using the fields defined in the schema.
        """;

    public static string CriticUser(string finalSummary, string timeline, string finalNotebook)
    {
        return $"""
            FINAL SUMMARY
            <<<
            {finalSummary}
            >>>

            TIMELINE
            Format: Day X: <commands> -> <key resolution signals>
            <<<
            {timeline}
            >>>

            FINAL NOTEBOOK
            <<<
            {finalNotebook}
            >>>

            Write the postmortem covering: outcome, cause, three impactful decisions,
            strategy, what felt unclear, what felt fair vs unfair, suggestions, and
            what you would try next run.
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
                    ["next_run"] = new JsonObject { ["type"] = "string", ["description"] = "What the player would try differently next run." }
                },
                ["required"] = new JsonArray("outcome", "cause", "impactful_decisions", "strategy", "unclear", "fair_vs_unfair", "suggestions", "next_run"),
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
