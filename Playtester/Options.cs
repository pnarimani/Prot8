using System.Text;
using Prot8.Simulation;

namespace Playtester;

internal sealed class PlaytesterOptions
{
    public string BaseUrl { get; private set; } = "http://localhost:1234/v1";

    public string Model { get; private set; } = "local-model";

    public string? ApiKey { get; private set; }

    public int? Seed { get; private set; }

    public string TutorialPath { get; private set; } = "Playtester/tutorial.md";

    public string ChatCompletionsUrl
    {
        get
        {
            var trimmed = BaseUrl.TrimEnd('/');
            if (trimmed.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed;
            }

            if (trimmed.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
            {
                return $"{trimmed}/chat/completions";
            }

            return $"{trimmed}/v1/chat/completions";
        }
    }

    public static PlaytesterOptions Parse(string[] args)
    {
        var options = new PlaytesterOptions();

        foreach (var arg in args)
        {
            if (arg.StartsWith("--base-url=", StringComparison.OrdinalIgnoreCase))
            {
                options.BaseUrl = arg.Substring("--base-url=".Length);
                continue;
            }

            if (arg.StartsWith("--model=", StringComparison.OrdinalIgnoreCase))
            {
                options.Model = arg.Substring("--model=".Length);
                continue;
            }

            if (arg.StartsWith("--seed=", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(arg.Substring("--seed=".Length), out var seed))
            {
                options.Seed = seed;
                continue;
            }

            if (arg.StartsWith("--tutorial=", StringComparison.OrdinalIgnoreCase))
            {
                options.TutorialPath = arg.Substring("--tutorial=".Length);
            }
        }

        var envApiKey = Environment.GetEnvironmentVariable("LMSTUDIO_API_KEY");
        if (!string.IsNullOrWhiteSpace(envApiKey))
        {
            options.ApiKey = envApiKey;
        }

        return options;
    }

    public string ResolveTutorialPath()
    {
        if (Path.IsPathRooted(TutorialPath))
        {
            return TutorialPath;
        }

        return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), TutorialPath));
    }
}

internal static class PostmortemWriter
{
    public static string BuildDocument(GameState state, string postmortemResponse)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Playtester Postmortem");
        builder.AppendLine();
        builder.AppendLine($"Outcome: {(state.Survived ? "Survived Day 40" : state.GameOverCause.ToString())}");
        builder.AppendLine($"Final Day: {state.Day}");
        builder.AppendLine($"Final Morale: {state.Morale}");
        builder.AppendLine($"Final Unrest: {state.Unrest}");
        builder.AppendLine($"Final Sickness: {state.Sickness}");
        builder.AppendLine();
        builder.AppendLine("## Agent Analysis");
        builder.AppendLine();
        builder.AppendLine(postmortemResponse.Trim());
        return builder.ToString();
    }
}