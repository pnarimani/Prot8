using Prot8.Cli;
using Prot8.Cli.Input;
using Prot8.Cli.Output;
using Prot8.Constants;
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
    var report = engine.StartDay();
    
    var vm = vmFactory.CreateDayStartViewModel();
    
    renderer.RenderDayStart(vm);

    while (vm.CurrentEvent is { } evt)
    {
        var choices = input.ReadEventResponses(evt);
        engine.ApplyEventResponses(report, choices);

        vm = vmFactory.CreateDayStartViewModel();
        renderer.RenderDayStart(vm);
    }

    var dayPlan = input.ReadDayPlan(renderer);
    var action = dayPlan.Action;

    engine.ResolveDay(action, report);

    var dayReportVm = GameViewModelFactory.ToDayReportViewModel(state, report);
    renderer.RenderDayReport(vm, dayReportVm);
    telemetry.LogDay(action, report);

    // Night phase
    if (GameBalance.EnableNightPhase && !state.GameOver)
    {
        var nightPlan = input.ReadNightPlan(renderer);
        engine.ResolveNight(nightPlan);
    }
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