using System.IO;
using System.Text;
using System.Text.Json.Nodes;
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
    var commanderJson = await llm.ChatAsync(
        AgentPrompts.CommanderSystem, commanderPrompt,
        temperature: 0.4, responseFormat: AgentPrompts.CommanderResponseFormat);

    // Parse commands from JSON
    var commandLines = ParseCommanderJson(commanderJson);
    Console.WriteLine($"[AI] Commander issued {commandLines.Count} command(s).");

    var executedCommands = new StringBuilder();
    foreach (var line in commandLines)
    {
        if (string.IsNullOrEmpty(line)) continue;
        if (string.Equals(line, "end_day", StringComparison.OrdinalIgnoreCase)) break;

        ConsoleInputReader.TryExecuteCommand(state, allocation, ref action, line, out var msg, out var endDay);
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
        Console.WriteLine($"[AI] Calling Scribe for Day {state.Day}...");
        var scribePrompt = AgentPrompts.ScribeUser(notebook, daySnapshot, cmds, resolutionLog);
        var scribeJson = await llm.ChatAsync(
            AgentPrompts.ScribeSystem, scribePrompt,
            temperature: 0.3, responseFormat: AgentPrompts.ScribeResponseFormat);

        notebook = FormatNotebook(scribeJson);
        Console.WriteLine($"[AI] Notebook updated ({notebook.Length} chars).\n");

        state.Day += 1;
    }
}

// Render final
var finalSummary = RenderToString(w => new ConsoleRenderer(w).RenderFinal(state));
Console.Write(finalSummary);
telemetry.LogFinal(state);

// Call Critic for postmortem
Console.WriteLine("[AI] Calling Critic for postmortem...");
var criticPrompt = AgentPrompts.CriticUser(finalSummary, timeline.ToString(), notebook);
var criticJson = await llm.ChatAsync(
    AgentPrompts.CriticSystem, criticPrompt,
    temperature: 0.5, responseFormat: AgentPrompts.CriticResponseFormat);

var postmortem = FormatPostmortem(criticJson);

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

static List<string> ParseCommanderJson(string json)
{
    try
    {
        var node = JsonNode.Parse(json);
        var array = node?["commands"]?.AsArray();
        if (array is null) return [];
        return [.. array.Select(x => x?.GetValue<string>() ?? "").Where(s => s.Length > 0)];
    }
    catch
    {
        Console.WriteLine($"[AI] Warning: failed to parse Commander JSON, treating as empty.");
        return [];
    }
}

static string FormatNotebook(string json)
{
    try
    {
        var node = JsonNode.Parse(json);
        if (node is null) return json;

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
            sb.AppendLine($"- {item?.GetValue<string>()}");
    }
    sb.AppendLine();
}

static string FormatPostmortem(string json)
{
    try
    {
        var node = JsonNode.Parse(json);
        if (node is null) return json;

        var sb = new StringBuilder();
        sb.AppendLine($"## 1) Outcome");
        sb.AppendLine(node["outcome"]?.GetValue<string>());
        sb.AppendLine();
        sb.AppendLine("## 2) What I believe caused the result");
        sb.AppendLine(node["cause"]?.GetValue<string>());
        sb.AppendLine();
        sb.AppendLine("## 3) Three most impactful decisions");
        var decisions = node["impactful_decisions"]?.AsArray();
        if (decisions is not null)
            foreach (var d in decisions)
                sb.AppendLine($"- {d?.GetValue<string>()}");
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
        return sb.ToString();
    }
    catch
    {
        return json;
    }
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
