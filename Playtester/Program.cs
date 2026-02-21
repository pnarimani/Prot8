using Playtester;
using Prot8.Simulation;
using Prot8.Telemetry;

var options = PlaytesterOptions.Parse(args);
var tutorialPath = options.ResolveTutorialPath();
if (!File.Exists(tutorialPath))
{
    Console.WriteLine($"Tutorial prompt file not found: {tutorialPath}");
    return 1;
}

var tutorialPrompt = await File.ReadAllTextAsync(tutorialPath);
if (string.IsNullOrWhiteSpace(tutorialPrompt))
{
    Console.WriteLine($"Tutorial prompt file is empty: {tutorialPath}");
    return 1;
}

var lmClient = new LmStudioClient(options);
var state = new GameState(options.Seed);
var engine = new GameSimulationEngine();
using var telemetry = new RunTelemetryWriter(options.Seed);

Console.WriteLine($"Playtester telemetry: {telemetry.FilePath}");
Console.WriteLine($"LMStudio endpoint: {options.ChatCompletionsUrl}");
Console.WriteLine($"LMStudio model: {options.Model}");

var previousTurnFeedback = "No previous turn feedback yet.";
var turnHistory = new List<string>();

while (!state.GameOver)
{
    var pendingPlan = AiTurnExecutor.BuildInitialPlan(state);
    var attemptFeedback = previousTurnFeedback;
    var attempt = 1;

    while (true)
    {
        var turnPrompt = PromptBuilder.BuildTurnPrompt(state, pendingPlan, attemptFeedback, attempt);

        var aiRawResponse = await lmClient.RequestJsonActionsAsync(tutorialPrompt, turnPrompt);
        var aiResponse = AiPlannerResponse.Parse(aiRawResponse, out var parseWarning);

        var execution = AiTurnExecutor.Execute(state, pendingPlan, aiResponse.Actions, parseWarning);
        attemptFeedback = PromptBuilder.BuildAttemptFeedback(state, pendingPlan, execution, aiRawResponse, attempt);

        Console.WriteLine($"Day {state.Day} attempt {attempt}. Executed {execution.Executed.Count} action(s), skipped {execution.Skipped.Count} action(s), end_day accepted: {execution.EndDayAccepted}.");

        if (!execution.EndDayAccepted)
        {
            attempt += 1;
            continue;
        }

        state.Allocation = pendingPlan.Allocation;
        var report = engine.ResolveDay(state, pendingPlan.ActionChoice);
        telemetry.LogDay(state, pendingPlan.ActionChoice, report);

        var resolvedFeedback = PromptBuilder.BuildTurnFeedback(state, report, execution, aiRawResponse);
        turnHistory.Add(resolvedFeedback);
        previousTurnFeedback = resolvedFeedback;

        Console.WriteLine($"Day {state.Day} complete.");
        if (!state.GameOver)
        {
            state.Day += 1;
        }

        break;
    }
}

telemetry.LogFinal(state);

var postmortemPrompt = PromptBuilder.BuildPostmortemPrompt(state, turnHistory);
var postmortemResponse = await lmClient.RequestTextAsync(tutorialPrompt, postmortemPrompt);

var telemetryDirectory = Path.GetDirectoryName(telemetry.FilePath) ?? ".";
var telemetryBaseName = Path.GetFileNameWithoutExtension(telemetry.FilePath);
var postmortemPath = Path.Combine(telemetryDirectory, $"{telemetryBaseName}_postmortem.md");

await File.WriteAllTextAsync(postmortemPath, PostmortemWriter.BuildDocument(state, postmortemResponse));

Console.WriteLine($"Postmortem written to: {postmortemPath}");
return 0;
