using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Prot8.Cli;
using Prot8.Cli.Commands;
using Prot8.Events;
using Prot8.Simulation;
using Prot8.Telemetry;
using Spectre.Console;

namespace Playtester;

public class CommanderScribeRunner
{
    public static async Task RunAsync(PlaytesterConfig config)
    {
        using var llm = new LmStudioClient(config.Endpoint, config.Model);

        var previousRunLearnings = "";

        var notebook = "(empty - first day)";

        var jsonSerializationOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        };

        for (var runIndex = 0; runIndex < 100; runIndex++)
        {
            Console.WriteLine($"\n{new string('=', 40)} RUN {runIndex + 1} / 100 {new string('=', 40)}\n");

            var state = new GameState(config.Seed);
            var engine = new GameSimulationEngine(state);
            using var telemetry = new RunTelemetryWriter(state, config.Seed);
            notebook = "(empty - first day)";

            var timeline = new StringBuilder();

            while (!state.GameOver)
            {
                // Capture day snapshot
                var dayStartVm = new GameViewModelFactory(state).Create();
                var daySnapshot = JsonSerializer.Serialize(dayStartVm, jsonSerializationOptions);
                Console.Write(daySnapshot);

                // Call Commander (with retry on invalid commands)
                TurnActionChoice action = new();
                const int maxCommanderRetries = 3;
                var executedCommands = new StringBuilder();
                string? commanderValidationErrors = null;

                for (var attempt = 1; attempt <= maxCommanderRetries; attempt++)
                {
                    // Reset allocation and action for this attempt
                    state.Allocation.Clear();
                    action = new TurnActionChoice();
                    executedCommands.Clear();

                    Console.WriteLine(attempt == 1
                        ? $"[AI] Calling Commander for Day {state.Day}..."
                        : $"[AI] Retrying Commander for Day {state.Day} (attempt {attempt}/{maxCommanderRetries})...");

                    var commanderPrompt =
                        Agents.CommanderUser(daySnapshot, notebook, previousRunLearnings, commanderValidationErrors);
                    var commanderJson = await llm.ChatAsync(
                        Agents.CommanderSystem, commanderPrompt, Agents.CommanderResponseFormat, 0.4);

                    var commandList = Parsing.ParseCommanderCommands(commanderJson, jsonSerializationOptions);
                    Console.WriteLine($"[AI] Commander issued {commandList.Count} command(s).");

                    var invalidCommands = new StringBuilder();
                    var hasInvalid = false;

                    foreach (var command in commandList)
                    {
                        var commandJson = JsonSerializer.Serialize(command);
                        var context = new CommandContext(state, action);
                        var result = command.Execute(context);
                        action = context.Action;
                        Console.WriteLine($"  > {commandJson}  => {result.Message}");

                        if (!result.Success)
                        {
                            invalidCommands.AppendLine($"  Command {commandJson} was rejected: {result.Message}");
                            hasInvalid = true;
                        }
                        else
                        {
                            executedCommands.AppendLine(commandJson);
                        }

                        if (result.EndDayRequested)
                        {
                            break;
                        }
                    }

                    if (!hasInvalid)
                    {
                        commanderValidationErrors = null;
                        break; // all commands valid — proceed
                    }

                    commanderValidationErrors = invalidCommands.ToString().TrimEnd();
                    Console.WriteLine(
                        $"[AI] Commander issued invalid command(s); requesting corrections:\n{commanderValidationErrors}");

                    if (attempt == maxCommanderRetries)
                    {
                        Console.WriteLine("[AI] Max retries reached. Proceeding with valid commands only.");
                    }
                }

                var report = engine.ResolveDay(action);

                if (report.PendingResponses.Count > 0)
                {
                    var defaultChoices = report.PendingResponses
                        .Select(p => new EventResponseChoice(p.Event.Id, p.Responses[^1].Id))
                        .ToList();
                    GameSimulationEngine.ApplyEventResponses(state, report, defaultChoices);
                }

                // Capture resolution log
                var dayReportVm = GameViewModelFactory.ToDayReportViewModel(state, report);
                var resolutionLog = JsonSerializer.Serialize(dayReportVm, jsonSerializationOptions);
                Console.Write(resolutionLog);

                telemetry.LogDay(action, report);

                // Build timeline entry with full day state
                var cmds = executedCommands.ToString().Trim();
                var signals = BuildSignals(state, report);
                timeline.AppendLine($"--- Day {state.Day} ---");
                // timeline.AppendLine(daySnapshot.Trim());
                timeline.AppendLine($"Commands: {cmds}");
                timeline.AppendLine($"Signals: {signals}");
                timeline.AppendLine();

                if (!state.GameOver)
                {
                    // Call Scribe
                    Console.WriteLine($"[AI] Calling Scribe for Day {state.Day}...");
                    var scribePrompt =
                        Agents.ScribeUser(notebook, daySnapshot, cmds, resolutionLog, previousRunLearnings);
                    var scribeJson = await llm.ChatAsync(
                        Agents.ScribeSystem, scribePrompt, Agents.ScribeResponseFormat, 0.3);

                    notebook = FormatNotebook(scribeJson);
                    Console.WriteLine($"[AI] Notebook updated ({notebook.Length} chars).\n");
                }
            }

// Render final
            var gameOverVm = GameViewModelFactory.ToGameOverViewModel(state);
            var finalSummary = JsonSerializer.Serialize(gameOverVm, jsonSerializationOptions);
            Console.Write(finalSummary);
            telemetry.LogFinal(state);

// Call Critic for postmortem
            Console.WriteLine("[AI] Calling Critic for postmortem...");
            var criticPrompt = Agents.CriticUser(finalSummary, timeline.ToString(), notebook, previousRunLearnings);
            var criticJson = await llm.ChatAsync(
                Agents.CriticSystem, criticPrompt, Agents.CriticResponseFormat, 0.5);

            var postmortem = FormatPostmortem(criticJson);

// Save postmortem next to telemetry
            var postmortemPath = Path.ChangeExtension(telemetry.FilePath, ".postmortem.md");
            await File.WriteAllTextAsync(postmortemPath, postmortem);

            Console.WriteLine($"\n{postmortem}");
            Console.WriteLine($"\nTelemetry:  {telemetry.FilePath}");
            Console.WriteLine($"Postmortem: {postmortemPath}");

            previousRunLearnings = ExtractLearnings(criticJson);
        }


        static string RenderToString(Action<IAnsiConsole> render)
        {
            using var sw = new StringWriter();
            var console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(sw),
                ColorSystem = ColorSystemSupport.NoColors,
                Interactive = InteractionSupport.No
            });
            render(console);
            return sw.ToString();
        }


        static string FormatNotebook(string json)
        {
            try
            {
                var node = JsonNode.Parse(json);
                if (node is null)
                {
                    return json;
                }

                var sb = new StringBuilder();
                AppendSection(sb, node, "hypotheses", "HYPOTHESES");
                AppendSection(sb, node, "observations", "OBSERVATIONS");
                AppendSection(sb, node, "open_questions", "OPEN QUESTIONS");
                AppendSection(sb, node, "plan", "PLAN");
                return sb.ToString().TrimEnd();
            }
            catch
            {
                return json;
            }
        }

        static void AppendSection(StringBuilder sb, JsonNode node, string key, string header)
        {
            sb.AppendLine(header);
            var arr = node[key]?.AsArray();
            if (arr is null || arr.Count == 0)
            {
                sb.AppendLine("- (none)");
            }
            else
            {
                foreach (var item in arr)
                {
                    sb.AppendLine($"- {item?.GetValue<string>()}");
                }
            }

            sb.AppendLine();
        }

        static string FormatPostmortem(string json)
        {
            try
            {
                var node = JsonNode.Parse(json);
                if (node is null)
                {
                    return json;
                }

                var sb = new StringBuilder();
                sb.AppendLine("## 1) Outcome");
                sb.AppendLine(node["outcome"]?.GetValue<string>());
                sb.AppendLine();
                sb.AppendLine("## 2) What I believe caused the result");
                sb.AppendLine(node["cause"]?.GetValue<string>());
                sb.AppendLine();
                sb.AppendLine("## 3) Three most impactful decisions");
                var decisions = node["impactful_decisions"]?.AsArray();
                if (decisions is not null)
                {
                    foreach (var d in decisions)
                    {
                        sb.AppendLine($"- {d?.GetValue<string>()}");
                    }
                }

                sb.AppendLine();
                sb.AppendLine("## 4) Strategy I converged on");
                sb.AppendLine(node["strategy"]?.GetValue<string>());
                sb.AppendLine();
                sb.AppendLine("## 5) What felt unclear or confusing");
                sb.AppendLine(node["unclear"]?.GetValue<string>());
                sb.AppendLine();
                sb.AppendLine("## 6) What felt fair vs unfair");
                sb.AppendLine(node["fair_vs_unfair"]?.GetValue<string>());
                sb.AppendLine();
                sb.AppendLine("## 7) Changes I would suggest");
                sb.AppendLine(node["suggestions"]?.GetValue<string>());
                sb.AppendLine();
                sb.AppendLine("## 8) What I would try next run");
                sb.AppendLine(node["next_run"]?.GetValue<string>());
                sb.AppendLine();
                sb.AppendLine("## 9) 10 Learnings for the next run");
                var learnings = node["learnings"]?.AsArray();
                if (learnings is not null)
                {
                    for (var i = 0; i < learnings.Count; i++)
                    {
                        sb.AppendLine($"{i + 1}. {learnings[i]?.GetValue<string>()}");
                    }
                }

                sb.AppendLine();
                sb.AppendLine("## 10) Did the commander do better than the previous run?");
                sb.AppendLine(node["better_than_previous"]?.GetValue<string>());
                return sb.ToString();
            }
            catch
            {
                return json;
            }
        }

        static string ExtractLearnings(string json)
        {
            try
            {
                var node = JsonNode.Parse(json);
                var arr = node?["learnings"]?.AsArray();
                if (arr is null || arr.Count == 0)
                {
                    return "(no learnings available from previous run)";
                }

                var sb = new StringBuilder();
                for (var i = 0; i < arr.Count; i++)
                {
                    sb.AppendLine($"{i + 1}. {arr[i]?.GetValue<string>()}");
                }

                return sb.ToString().TrimEnd();
            }
            catch
            {
                return "(failed to extract learnings from previous run)";
            }
        }

        static string BuildSignals(GameState state, DayResolutionReport report)
        {
            var parts = new List<string>();
            if (report.FoodDeficitToday)
            {
                parts.Add("FOOD_DEFICIT");
            }

            if (report.WaterDeficitToday)
            {
                parts.Add("WATER_DEFICIT");
            }

            if (report.TriggeredEvents.Count > 0)
            {
                parts.Add(string.Join(", ", report.TriggeredEvents));
            }

            if (state.GameOver)
            {
                parts.Add($"GAME_OVER:{state.GameOverCause}");
            }

            if (parts.Count == 0)
            {
                parts.Add("OK");
            }

            return string.Join("; ", parts);
        }
    }
}