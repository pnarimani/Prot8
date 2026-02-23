using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Prot8.Cli;
using Prot8.Cli.Commands;
using Prot8.Cli.ViewModels;
using Prot8.Events;
using Prot8.Simulation;
using Prot8.Telemetry;

namespace Playtester;

public class OperatorAnalystRunner
{
    class RecentHistory
    {
        public JsonArray Last5Days { get; } = [];
    }

    class AnalystTimeline
    {
        public JsonArray Timeline { get; } = [];
    }

    public static async Task RunAsync(PlaytesterConfig config)
    {
        using var llm = new LmStudioClient(config.Endpoint, config.Model);

        var survivalGuide = "No Survival Guide";

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        };

        for (var runIndex = 0; runIndex < 100; runIndex++)
        {
            Console.WriteLine($"\n{new string('=', 40)} RUN {runIndex + 1} / 100 {new string('=', 40)}\n");

            var lastDayResolution = "";
            var recentHistory = new RecentHistory();
            var state = new GameState(config.Seed);
            var engine = new GameSimulationEngine(state);
            using var telemetry = new RunTelemetryWriter(state, config.Seed);
            var analystTimeline = new AnalystTimeline();
            var viewModelFactory = new GameViewModelFactory(state);

            while (!state.GameOver)
            {
                Console.WriteLine($"== Day {state.Day} ==");
                var dayStartVm = viewModelFactory.Create();
                var daySnapshot = JsonSerializer.Serialize(dayStartVm, jsonOptions);

                TurnActionChoice action = new();

                var operatorUser = Agents.Operator.GetUserPrompt(
                    survivalGuide,
                    lastDayResolution,
                    daySnapshot,
                    JsonSerializer.Serialize(recentHistory, jsonOptions)
                );

                var commanderJson = await llm.ChatAsync(
                    Agents.Operator.System,
                    operatorUser,
                    Agents.CommanderResponseFormat,
                    0.4
                );

                var commandList = Parsing.ParseCommanderCommands(commanderJson, jsonOptions);

                foreach (var command in commandList)
                {
                    var commandJson = JsonSerializer.Serialize(command);
                    var context = new CommandContext(state, action);
                    var result = command.Execute(context);
                    action = context.Action;
                    Console.WriteLine($"  > {commandJson}  => {result.Message}");

                    if (result.EndDayRequested)
                    {
                        break;
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

                var dayReportVm = GameViewModelFactory.ToDayReportViewModel(state, report);
                lastDayResolution = JsonSerializer.Serialize(dayReportVm, jsonOptions);

                telemetry.LogDay(action, report);

                var dayReportJsonNode = JsonSerializer.SerializeToNode(dayReportVm, jsonOptions);

                var history = new JsonObject
                {
                    ["day"] = state.Day - 1,
                    ["commands"] = JsonSerializer.SerializeToNode(commandList, jsonOptions),
                    ["resolution"] = dayReportJsonNode,
                };
                recentHistory.Last5Days.Add(history);

                if (recentHistory.Last5Days.Count > 5)
                {
                    recentHistory.Last5Days.RemoveAt(0);
                }

                analystTimeline.Timeline.Add(history.DeepClone());
            }

            var gameOverVm = GameViewModelFactory.ToGameOverViewModel(state);
            var summary = JsonSerializer.Serialize(gameOverVm, jsonOptions);
            Console.WriteLine(summary);
            telemetry.LogFinal(state);

            var analystUser = Agents.Analyst.GetUserPrompt(
                summary,
                analystTimeline.Timeline.ToJsonString(),
                survivalGuide
            );
            
            var postmortem = await llm.ChatAsync(
                Agents.Analyst.System,
                analystUser,
                Agents.Analyst.ResponseFormat,
                0.5
            );

            survivalGuide = ParseSurvivalGuide(postmortem);
            
            var postmortemPath = Path.ChangeExtension(telemetry.FilePath, ".postmortem.md");
            await File.WriteAllTextAsync(postmortemPath, postmortem);

            Console.WriteLine($"\nTelemetry:  {telemetry.FilePath}");
            Console.WriteLine($"Postmortem: {postmortemPath}");
            Console.WriteLine("=====");
            Console.WriteLine(postmortem);
            Console.WriteLine("=====");
        }
    }

    static string ParseSurvivalGuide(string postmortem)
    {
        try
        {
            var json = JsonNode.Parse(postmortem);
            return json?["survival_guide"]?.ToString() ?? "(failed to parse survival guide)";
        }
        catch
        {
            return "(failed to parse survival guide)";
        }
    }
}