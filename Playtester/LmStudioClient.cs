using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Playtester;

internal sealed class LmStudioClient
{
    private readonly PlaytesterOptions _options;
    private readonly HttpClient _httpClient;

    public LmStudioClient(PlaytesterOptions options)
    {
        _options = options;
        _httpClient = new HttpClient();

        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }
    }

    public Task<string> RequestJsonActionsAsync(string tutorialPrompt, string userPrompt)
    {
        return RequestCompletionAsync(tutorialPrompt, userPrompt, requestJsonResponse: true);
    }

    public Task<string> RequestTextAsync(string tutorialPrompt, string userPrompt)
    {
        return RequestCompletionAsync(tutorialPrompt, userPrompt, requestJsonResponse: false);
    }

    private async Task<string> RequestCompletionAsync(string tutorialPrompt, string userPrompt, bool requestJsonResponse)
    {
        object payload;
        if (requestJsonResponse)
        {
            payload = new
            {
                model = _options.Model,
                temperature = 0.2,
                max_tokens = 1200,
                response_format = BuildJsonSchemaResponseFormat(),
                messages = new object[]
                {
                    new { role = "system", content = tutorialPrompt },
                    new { role = "user", content = userPrompt }
                }
            };
        }
        else
        {
            payload = new
            {
                model = _options.Model,
                temperature = 0.2,
                max_tokens = 1200,
                messages = new object[]
                {
                    new { role = "system", content = tutorialPrompt },
                    new { role = "user", content = userPrompt }
                }
            };
        }

        var json = JsonSerializer.Serialize(payload);
        using var requestContent = new StringContent(json, Encoding.UTF8);
        requestContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        using var request = new HttpRequestMessage(HttpMethod.Post, _options.ChatCompletionsUrl)
        {
            Content = requestContent
        };

        using var response = await _httpClient.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"LMStudio request failed ({(int)response.StatusCode}): {body}");
        }

        var responseContent = ExtractMessageContent(body);
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            throw new InvalidOperationException("LMStudio returned empty content.");
        }

        return responseContent.Trim();
    }

    private static object BuildJsonSchemaResponseFormat()
    {
        return new
        {
            type = "json_schema",
            json_schema = new
            {
                name = "playtester_actions",
                schema = new
                {
                    type = "object",
                    properties = new
                    {
                        actions = new
                        {
                            type = "array",
                            items = new
                            {
                                type = "object",
                                properties = new
                                {
                                    type = new { type = "string" },
                                    target = new { type = "string" },
                                    workers = new { type = "integer" },
                                    zone = new { type = "string" }
                                },
                                required = new[] { "type" },
                                additionalProperties = true
                            }
                        },
                        reasoning = new { type = "string" }
                    },
                    required = new[] { "actions" },
                    additionalProperties = true
                }
            }
        };
    }

    private static string ExtractMessageContent(string responseBody)
    {
        using var document = JsonDocument.Parse(responseBody);
        if (!document.RootElement.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
        {
            throw new InvalidOperationException($"LMStudio response missing choices: {responseBody}");
        }

        var firstChoice = choices[0];
        if (!firstChoice.TryGetProperty("message", out var message))
        {
            throw new InvalidOperationException($"LMStudio response missing message: {responseBody}");
        }

        if (!message.TryGetProperty("content", out var content))
        {
            throw new InvalidOperationException($"LMStudio response missing message content: {responseBody}");
        }

        if (content.ValueKind == JsonValueKind.String)
        {
            return content.GetString() ?? string.Empty;
        }

        if (content.ValueKind == JsonValueKind.Array)
        {
            var builder = new StringBuilder();
            foreach (var item in content.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object
                    && item.TryGetProperty("text", out var text)
                    && text.ValueKind == JsonValueKind.String)
                {
                    builder.AppendLine(text.GetString());
                }
            }

            return builder.ToString();
        }

        return content.GetRawText();
    }
}
