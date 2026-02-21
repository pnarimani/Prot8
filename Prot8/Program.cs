using Prot8.Cli.Input;
using Prot8.Cli.Output;
using Prot8.Simulation;
using Prot8.Telemetry;

var seed = TryParseSeed(args);
var state = new GameState(seed);
var engine = new GameSimulationEngine();
var renderer = new ConsoleRenderer();
var input = new ConsoleInputReader();

using var telemetry = new RunTelemetryWriter(seed);

while (!state.GameOver)
{
    renderer.RenderDayStart(state);
    var dayPlan = input.ReadDayPlan(state, renderer);
    state.Allocation = dayPlan.Allocation;
    var action = dayPlan.Action;

    var report = engine.ResolveDay(state, action);
    renderer.RenderDayReport(state, report);
    telemetry.LogDay(state, action, report);

    if (!state.GameOver)
    {
        state.Day += 1;
    }
}

renderer.RenderFinal(state);
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
