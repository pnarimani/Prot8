using System.Text.Json;

namespace Playtester;

internal sealed class AiPlannerResponse
{
    public IReadOnlyList<AiPlannedAction> Actions { get; private set; } = Array.Empty<AiPlannedAction>();

    public static AiPlannerResponse Parse(string rawResponse, out string? parseWarning)
    {
        parseWarning = null;

        var cleaned = CleanupJsonResponse(rawResponse);
        try
        {
            using var document = JsonDocument.Parse(cleaned);
            if (!document.RootElement.TryGetProperty("actions", out var actionsElement) || actionsElement.ValueKind != JsonValueKind.Array)
            {
                parseWarning = "Agent response did not include a valid 'actions' array.";
                return new AiPlannerResponse();
            }

            var actions = new List<AiPlannedAction>();
            foreach (var item in actionsElement.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                var action = new AiPlannedAction
                {
                    Type = ReadString(item, "type"),
                    Target = ReadString(item, "target") ?? ReadString(item, "id") ?? ReadString(item, "ref"),
                    Zone = ReadString(item, "zone")
                };

                if (TryReadInt(item, "workers", out var workers))
                {
                    action.Workers = workers;
                }
                else if (TryReadInt(item, "value", out var value))
                {
                    action.Workers = value;
                }

                actions.Add(action);
            }

            return new AiPlannerResponse { Actions = actions };
        }
        catch (Exception exception)
        {
            parseWarning = $"Agent response JSON parsing failed: {exception.Message}";
            return new AiPlannerResponse();
        }
    }

    private static string CleanupJsonResponse(string input)
    {
        var trimmed = input.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var firstNewLine = trimmed.IndexOf('\n');
            if (firstNewLine >= 0)
            {
                trimmed = trimmed.Substring(firstNewLine + 1);
            }

            var closingFenceIndex = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (closingFenceIndex >= 0)
            {
                trimmed = trimmed.Substring(0, closingFenceIndex);
            }
        }

        return trimmed.Trim();
    }

    private static string? ReadString(JsonElement element, string property)
    {
        foreach (var child in element.EnumerateObject())
        {
            if (!string.Equals(child.Name, property, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return child.Value.ValueKind switch
            {
                JsonValueKind.String => child.Value.GetString(),
                JsonValueKind.Number => child.Value.GetRawText(),
                _ => null
            };
        }

        return null;
    }

    private static bool TryReadInt(JsonElement element, string property, out int value)
    {
        value = 0;
        foreach (var child in element.EnumerateObject())
        {
            if (!string.Equals(child.Name, property, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (child.Value.ValueKind == JsonValueKind.Number && child.Value.TryGetInt32(out value))
            {
                return true;
            }

            if (child.Value.ValueKind == JsonValueKind.String && int.TryParse(child.Value.GetString(), out value))
            {
                return true;
            }

            return false;
        }

        return false;
    }
}

internal sealed class AiPlannedAction
{
    public string? Type { get; set; }

    public string? Target { get; set; }

    public int? Workers { get; set; }

    public string? Zone { get; set; }
}

internal sealed class TurnExecutionResult
{
    public TurnExecutionResult(Prot8.Jobs.JobAllocation allocation, Prot8.Simulation.TurnActionChoice actionChoice, List<string> executed, List<string> skipped)
    {
        Allocation = allocation;
        ActionChoice = actionChoice;
        Executed = executed;
        Skipped = skipped;
    }

    public Prot8.Jobs.JobAllocation Allocation { get; }

    public Prot8.Simulation.TurnActionChoice ActionChoice { get; }

    public List<string> Executed { get; }

    public List<string> Skipped { get; }
}