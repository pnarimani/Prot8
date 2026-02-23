using Prot8.Cli;
using Prot8.Cli.Input;
using Prot8.Cli.Output;
using Prot8.Cli.ViewModels;
using Prot8.Simulation;
using Prot8.Telemetry;
using Spectre.Console;

var seed = TryParseSeed(args);
var state = new GameState(seed);
var engine = new GameSimulationEngine(state);
var renderer = new ConsoleRenderer(AnsiConsole.Console);
var input = new ConsoleInputReader(new CommandParser());

using var telemetry = new RunTelemetryWriter(state, seed);

while (!state.GameOver)
{
    GameSimulationEngine.RollDailyDisruption(state);
    var dayStartVm = new GameViewModelFactory(state).Create();
    renderer.RenderDayStart(dayStartVm);
    var dayPlan = input.ReadDayPlan(state, renderer);
    state.Allocation = dayPlan.Allocation;
    var action = dayPlan.Action;

    var report = engine.ResolveDay(action);

    if (report.PendingResponses.Count > 0)
    {
        var choices = input.ReadEventResponses(report.PendingResponses, renderer);
        GameSimulationEngine.ApplyEventResponses(state, report, choices);
    }

    var dayReportVm = GameViewModelFactory.ToDayReportViewModel(state, report);
    renderer.RenderDayReport(dayReportVm);
    telemetry.LogDay(action, report);
}

var gameOverVm = GameViewModelFactory.ToGameOverViewModel(state);
renderer.RenderFinal(gameOverVm);
telemetry.LogFinal(state);
Console.WriteLine($"Telemetry written to: {telemetry.FilePath}");

static int? TryParseSeed(string[] args)
{
    foreach (var arg in args)
    {
        if (!arg.StartsWith("--seed=", StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        var value = arg.Substring("--seed=".Length);
        if (int.TryParse(value, out var seed))
        {
            return seed;
        }
    }

    return null;
}
