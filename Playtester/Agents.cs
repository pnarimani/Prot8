namespace Playtester;

public static class Agents
{
    public static class Operator
    {
        public const string System =
            """
            You are playing a brutal, turn-based survival siege game. Your goal is to survive for 40 days.
            You cannot stabilize; you must make difficult sacrifices every day to delay failure.

            Mechanics:
                Orders: Take effect immediately.
                Laws: Take effect permanently.
                Missions: Take several days to resolve and have multiple outcomes.
            
            You may only choose one of Orders, Laws and Missions to activate on one day.
            For example: You CANNOT activate an order and a law on the same day.
            
            You may not assign more workers to a building than its max capacity or `IdleWorkersForAssignment`
            If you want to move workers between buildings:
            1. Remove workers from one building with negative delta_workers
            2. Assign workers to another building with positive delta_workers
            
            The required workers for a mission need to be IDLE WORKERS BEFORE YOU ACTIVATE A MISSION.
            
            YOU CANNOT execute an order if OrderCooldownDaysRemaining is greater than 0.

            You have access to a "Survival Guide" written by your past iterations. You must treat the rules in this guide as absolute truth. If the guide tells you not to do something, do not do it.

            Respond with a JSON object. The "commands" field is an ordered array of JSON-serialized command objects. 
            Each command object has a "type" discriminator field and the command's own fields:
            ```
                { "type": "add_workers",       "building_id": "<BuildingId>", "delta_workers": <positive delta> }
                { "type": "remove_workers",    "building_id": "<BuildingId>", "delta_workers": <positive delta> }
                { "type": "enact_law",         "law_id": "<LawId>" }
                { "type": "issue_order",       "order_id": "<OrderId>" }
                { "type": "start_mission",     "mission_id": "<MissionId>" }
                { "type": "clear_action" }
                { "type": "end_day" }  — must be the last command.
            ```
            """;

        const string User =
            """
            Current Survival Guide:
            ```
            {0}
            ```
            
            Recent History:
            ```
            {3}
            ```
            
            Last Day Resolution:
            ```json
            {1}
            ```

            Current Game State:
            ```json
            {2}
            ```

            Based on the guide and the state, choose your action for today.
            """;

        public static string GetUserPrompt(string survivalGuide, string lastDayResolution, string gameState, string recentHistory)
        {
            return string.Format(User, survivalGuide, lastDayResolution, gameState, recentHistory);
        }
    }

    public static class Analyst
    {
        public const string System =
            """
            You are an expert forensic game analyst. Your job is to analyze the log of a failed run in a brutal survival siege game and update the "Survival Guide".
            The game features hidden, cascading consequences. A death on Day 15 due to "Starvation" is rarely caused by the action taken on Day 14. It is usually caused by a strategic error made days earlier (e.g., passing a law that drains food, or failing to send a mission).

            Your tasks:
            * Analyze the Game Log and the Cause of Death.
            * Trace back the cascading failure. Identify the exact day and action that doomed the run.
            * Analyze the current Survival Guide. Determine if a rule in the guide is incorrect and led to this death, or if a new rule needs to be added.
            * Rewrite the Survival Guide. The guide must be concise, heavily prioritized, and written as strict directives for the next player (e.g., "NEVER pass [Law X] before Day 5, it causes a food crash on Day 10").

            Output ONLY the updated contents of the Survival Guide in Markdown format.
            """;

        const string User =
            """
            Game Over Reason:
            ```
            {0}
            ```

            Timeline:
            ```
            {1}
            ```

            Current Survival Guide:
            ```
            {2}
            ```

            Provide the completely updated Survival Guide.
            """;

        public static string GetUserPrompt(string gameOverReason, string timeline, string currentGuide)
        {
            return string.Format(User, gameOverReason, timeline, currentGuide);
        }
        
        
        public const string ResponseFormat =
            """
            {
                "type": "json_schema",
                "json_schema": {
                    "name": "scribe_notebook",
                    "strict": true,
                    "schema": {
                        "type": "object",
                        "properties": {
                            "survival_guide": {
                                "type": "string",
                                "description": "The entire survival guide"
                            }
                        },
                        "required": ["survival_guide"],
                        "additionalProperties": false
                    }
                }
            }
            """;
    }


    // ── Commander ────────────────────────────────────────────────────────────

    public const string CommanderSystem =
        """
        You are playing a siege survival city manager through a CLI interface.

        Your objective is to survive until Day 40.

        Rules:
        - Total assigned workers must not exceed the available workers shown.
        - Each day, You may select to queue ONLY one of Emergency Order, Law or Mission. 
        - Do not invent mechanics, rules, commands, or hidden information.
        - Respond with a JSON object. The "commands" field is an ordered array of JSON-serialized command objects.
          Each command object has a "type" discriminator field and the command's own fields:
            { "type": "add_workers",       "building_id": "<BuildingId>", "delta_workers": <positive int> }
            { "type": "remove_workers",    "building_id": "<BuildingId>", "delta_workers": <positive int> }
            { "type": "enact_law",         "law_id": "<LawId>" }
            { "type": "issue_order",       "order_id": "<OrderId>" }
            { "type": "start_mission",     "mission_id": "<MissionId>" }
            { "type": "end_day" }  — must be the last command.
        """;

    public static string CommanderUser(string daySnapshot, string notebook, string previousRunLearnings,
        string? validationErrors = null)
    {
        if (!string.IsNullOrEmpty(previousRunLearnings))
        {
            previousRunLearnings = "<previous_learnings>\n" + previousRunLearnings + "</previous_learnings>";
        }

        var errorSection = validationErrors is null
            ? ""
            : $"""
               VALIDATION ERRORS FROM YOUR PREVIOUS ATTEMPT
               The following commands were rejected. You must fix all of them before the day can proceed.
               ```
               {validationErrors}
               ```

               """;

        return $$"""
                 {{errorSection}}
                 ```json
                 {{daySnapshot}}
                 ```

                 <notebook>
                 {{notebook}}
                 </notebook>

                 {{previousRunLearnings}}

                 Win and Loss Conditions
                 - Win: reach Day 40.
                 - Lose: Keep integrity reaches 0 or below.
                 - Lose: Unrest rises above 85.
                 - Lose: Food and Water are both 0 for 2 consecutive days.

                 Respond with a JSON object. The "commands" field is an ordered array of JSON-serialized command objects.
                 The last command must be `{ "type": "end_day" }`.
                 """;
    }


    public const string CommanderResponseFormat =
        """
        {
            "type": "json_schema",
            "json_schema": {
                "name": "commander_response",
                "strict": true,
                "schema": {
                    "type": "object",
                    "properties": {
                        "strategy": {
                          "type": "string",
                          "description": "High-level description of the strategy guiding today's commands"
                        },
                        "commands": {
                          "type": "array",
                          "minItems": 1,
                          "description": "Ordered command objects. Last entry must have type=\"end_day\".",
                          "items": {
                            "oneOf": [
                              {
                                "type": "object",
                                "properties": {
                                  "type": { "const": "add_workers" },
                                  "building_id": { "type": "string" },
                                  "delta_workers": { "type": "integer" }
                                },
                                "required": ["type", "building_id", "delta_workers"],
                                "additionalProperties": false
                              },
                              {
                                  "type": "object",
                                  "properties": {
                                    "type": { "const": "remove_workers" },
                                    "building_id": { "type": "string" },
                                    "delta_workers": { "type": "integer" }
                                  },
                                  "required": ["type", "building_id", "delta_workers"],
                                  "additionalProperties": false
                                },
                              {
                                "type": "object",
                                "properties": {
                                  "type": { "const": "enact_law" },
                                  "law_id": { "type": "string" }
                                },
                                "required": ["type", "law_id"],
                                "additionalProperties": false
                              },
                              {
                                "type": "object",
                                "properties": {
                                  "type": { "const": "issue_order" },
                                  "order_id": { "type": "string" }
                                },
                                "required": ["type", "order_id"],
                                "additionalProperties": false
                              },
                              {
                                "type": "object",
                                "properties": {
                                  "type": { "const": "start_mission" },
                                  "mission_id": { "type": "string" }
                                },
                                "required": ["type", "mission_id"],
                                "additionalProperties": false
                              },
                              {
                                "type": "object",
                                "properties": { "type": { "const": "end_day" } },
                                "required": ["type"],
                                "additionalProperties": false
                              }
                            ]
                          }
                        }
                    },
                    "required": ["commands", "strategy"],
                    "additionalProperties": false
                }
            }
        }
        """;

    // ── Scribe ───────────────────────────────────────────────────────────────

    public const string ScribeSystem =
        """
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

    public static string ScribeUser(string previousNotebook, string daySnapshot, string commandsExecuted,
        string resolutionLog, string previousRunLearnings)
    {
        if (!string.IsNullOrEmpty(previousRunLearnings))
        {
            previousRunLearnings = "<previous_learnings>\n" + previousRunLearnings + "\n</previous_learnings>";
        }

        return $"""
                <old_notebook>
                {previousNotebook}
                </old_notebook>

                ```json
                {daySnapshot}
                ```

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

    public const string ScribeResponseFormat =
        """
        {
            "type": "json_schema",
            "json_schema": {
                "name": "scribe_notebook",
                "strict": true,
                "schema": {
                    "type": "object",
                    "properties": {
                        "hypotheses": {
                            "type": "array",
                            "description": "Current beliefs about how the game works.",
                            "items": { "type": "string" }
                        },
                        "observations": {
                            "type": "array",
                            "description": "Concrete things noticed this day.",
                            "items": { "type": "string" }
                        },
                        "open_questions": {
                            "type": "array",
                            "description": "Things still unknown or uncertain.",
                            "items": { "type": "string" }
                        },
                        "plan": {
                            "type": "array",
                            "description": "Intended strategy for upcoming days.",
                            "items": { "type": "string" }
                        }
                    },
                    "required": ["hypotheses", "observations", "open_questions", "plan"],
                    "additionalProperties": false
                }
            }
        }
        """;

    // ── Critic ───────────────────────────────────────────────────────────────

    public const string CriticSystem =
        """
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

    public static string CriticUser(string finalSummary, string timeline, string finalNotebook,
        string previousRunLearnings)
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

    public const string CriticResponseFormat =
        """
        {
            "type": "json_schema",
            "json_schema": {
                "name": "critic_postmortem",
                "strict": true,
                "schema": {
                    "type": "object",
                    "properties": {
                        "outcome": { "type": "string", "description": "One-sentence summary of how the run ended." },
                        "cause": { "type": "string", "description": "What the player believes caused the outcome." },
                        "impactful_decisions": {
                            "type": "array",
                            "description": "Exactly three decisions that most affected the run.",
                            "items": { "type": "string" },
                            "minItems": 3,
                            "maxItems": 3
                        },
                        "strategy": { "type": "string", "description": "The dominant strategy the player converged on, if any." },
                        "unclear": { "type": "string", "description": "What felt unclear or confusing during the run." },
                        "fair_vs_unfair": { "type": "string", "description": "What felt fair and what felt unfair." },
                        "suggestions": { "type": "string", "description": "Concrete changes the player would suggest." },
                        "next_run": { "type": "string", "description": "What the player would try differently next run." },
                        "learnings": {
                            "type": "array",
                            "description": "Exactly 10 actionable learnings for the commander to apply in the next run.",
                            "items": { "type": "string" },
                            "minItems": 10,
                            "maxItems": 10
                        },
                        "better_than_previous": {
                            "type": "string",
                            "description":
                                $"Assessment of whether the commander performed better than the previous run, with specific reasoning."
                        }
                    },
                    "required": ["outcome", "cause", "impactful_decisions", "strategy", "unclear",
                               "fair_vs_unfair", "suggestions", "next_run", "learnings", "better_than_previous"],
                    "additionalProperties": false
                }
            }
        }
        """;

}