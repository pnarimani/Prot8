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
var vmFactory = new GameViewModelFactory(state);
var input = new ConsoleInputReader(state, vmFactory, new CommandParser());

using var telemetry = new RunTelemetryWriter(state, seed);

while (!state.GameOver)
{
    engine.RollDailyDisruption();
    renderer.RenderDayStart(vmFactory.CreateDayStartViewModel());
    var dayPlan = input.ReadDayPlan(renderer);
    state.Allocation = dayPlan.Allocation;
    var action = dayPlan.Action;

    var report = engine.ResolveDay(action);

    if (report.PendingResponses.Count > 0)
    {
        var choices = input.ReadEventResponses(report.PendingResponses, renderer);
        engine.ApplyEventResponses(report, choices);
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
