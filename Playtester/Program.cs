using System.IO;
using System.Text;
using Playtester;
using Prot8.Cli.Input;
using Prot8.Cli.Output;
using Prot8.Jobs;
using Prot8.Simulation;
using Prot8.Telemetry;

var config = ParseArgs(args);

Console.WriteLine($"Prot8 AI Playtester");
Console.WriteLine($"  Endpoint: {config.Endpoint}");
Console.WriteLine($"  Model:    {(string.IsNullOrEmpty(config.Model) ? "(server default)" : config.Model)}");
Console.WriteLine($"  Seed:     {config.Seed?.ToString() ?? "random"}");
Console.WriteLine();

using var llm = new LmStudioClient(config.Endpoint, config.Model);

var state = new GameState(config.Seed);
var engine = new GameSimulationEngine();
using var telemetry = new RunTelemetryWriter(config.Seed);

var notebook = "(empty - first day)";
var timeline = new StringBuilder();

while (!state.GameOver)
{
    // Capture day snapshot
    var daySnapshot = RenderToString(w => new ConsoleRenderer(w).RenderDayStart(state));
    Console.Write(daySnapshot);

    // Build starting allocation
    var allocation = ConsoleInputReader.BuildStartingAllocation(state, out var adjustMsg);
    if (adjustMsg != null) Console.WriteLine(adjustMsg);
    var action = new TurnActionChoice();

    // Call Commander
    Console.WriteLine($"[AI] Calling Commander for Day {state.Day}...");
    var commanderPrompt = AgentPrompts.CommanderUser(daySnapshot, notebook);
    var commanderResponse = await llm.ChatAsync(AgentPrompts.CommanderSystem, commanderPrompt, temperature: 0.4);
    Console.WriteLine($"[AI] Commander response:\n{commanderResponse}\n");

    // Parse and execute commands
    var commandLines = ParseCommandLines(commanderResponse);
    var executedCommands = new StringBuilder();

    foreach (var line in commandLines)
    {
        if (string.Equals(line, "end_day", StringComparison.OrdinalIgnoreCase))
            break;

        var success = ConsoleInputReader.TryExecuteCommand(state, allocation, ref action, line, out var msg, out var endDay);
        Console.WriteLine($"  > {line} => {msg}");
        executedCommands.AppendLine(line);

        if (endDay) break;
    }

    // Finalize allocation and resolve
    ConsoleInputReader.FinalizeAllocation(state, allocation);
    state.Allocation = allocation;

    var report = engine.ResolveDay(state, action);

    // Capture resolution log
    var resolutionLog = RenderToString(w => new ConsoleRenderer(w).RenderDayReport(state, report));
    Console.Write(resolutionLog);

    telemetry.LogDay(state, action, report);

    // Build timeline entry
    var cmds = executedCommands.ToString().Trim();
    var signals = BuildSignals(state, report);
    timeline.AppendLine($"Day {state.Day}: {cmds} -> {signals}");

    if (!state.GameOver)
    {
        // Call Scribe
        Console.WriteLine($"\n[AI] Calling Scribe for Day {state.Day}...");
        var scribePrompt = AgentPrompts.ScribeUser(notebook, daySnapshot, cmds, resolutionLog);
        notebook = await llm.ChatAsync(AgentPrompts.ScribeSystem, scribePrompt, temperature: 0.3);
        // Trim notebook to ~1200 chars
        if (notebook.Length > 1500)
            notebook = notebook[..1500];
        Console.WriteLine($"[AI] Notebook updated ({notebook.Length} chars).\n");

        state.Day += 1;
    }
}

// Render final
var finalSummary = RenderToString(w => new ConsoleRenderer(w).RenderFinal(state));
Console.Write(finalSummary);
telemetry.LogFinal(state);

// Call Critic for postmortem
Console.WriteLine("\n[AI] Calling Critic for postmortem...");
var criticPrompt = AgentPrompts.CriticUser(finalSummary, timeline.ToString(), notebook);
var postmortem = await llm.ChatAsync(AgentPrompts.CriticSystem, criticPrompt, temperature: 0.5);

// Save postmortem next to telemetry
var postmortemPath = Path.ChangeExtension(telemetry.FilePath, ".postmortem.md");
await File.WriteAllTextAsync(postmortemPath, postmortem);

Console.WriteLine($"\n{postmortem}");
Console.WriteLine($"\nTelemetry:  {telemetry.FilePath}");
Console.WriteLine($"Postmortem: {postmortemPath}");

// --- Helper methods ---

static string RenderToString(Action<TextWriter> render)
{
    using var sw = new StringWriter();
    render(sw);
    return sw.ToString();
}

static List<string> ParseCommandLines(string response)
{
    var lines = new List<string>();
    foreach (var raw in response.Split('\n'))
    {
        var line = raw.Trim();
        if (line.Length == 0) continue;
        if (line.StartsWith('#')) continue;     // comment
        if (line.StartsWith("```")) continue;   // markdown fence
        lines.Add(line);
    }
    return lines;
}

static string BuildSignals(GameState state, DayResolutionReport report)
{
    var parts = new List<string>();
    if (report.FoodDeficitToday) parts.Add("FOOD_DEFICIT");
    if (report.WaterDeficitToday) parts.Add("WATER_DEFICIT");
    if (report.TriggeredEvents.Count > 0) parts.Add(string.Join(", ", report.TriggeredEvents));
    if (state.GameOver) parts.Add($"GAME_OVER:{state.GameOverCause}");
    if (parts.Count == 0) parts.Add("OK");
    return string.Join("; ", parts);
}

static PlaytesterConfig ParseArgs(string[] args)
{
    var config = new PlaytesterConfig();
    foreach (var arg in args)
    {
        if (arg.StartsWith("--seed=", StringComparison.OrdinalIgnoreCase))
        {
            if (int.TryParse(arg["--seed=".Length..], out var s))
                config.Seed = s;
        }
        else if (arg.StartsWith("--model=", StringComparison.OrdinalIgnoreCase))
        {
            config.Model = arg["--model=".Length..];
        }
        else if (arg.StartsWith("--endpoint=", StringComparison.OrdinalIgnoreCase))
        {
            config.Endpoint = arg["--endpoint=".Length..];
        }
    }
    return config;
}

class PlaytesterConfig
{
    public int? Seed { get; set; }
    public string? Model { get; set; }
    public string Endpoint { get; set; } = "http://localhost:1234";
}