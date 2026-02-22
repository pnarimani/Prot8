using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Playtester;

public sealed class LmStudioClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly string _model;

    public LmStudioClient(string endpoint = "http://localhost:1234", string? model = null)
    {
        _http = new HttpClient { BaseAddress = new Uri(endpoint), Timeout = TimeSpan.FromMinutes(5) };
        _model = model ?? "";
    }

    public async Task<string> ChatAsync(string systemPrompt, string userPrompt, double temperature = 0.7)
    {
        var request = new ChatRequest
        {
            Model = _model,
            Temperature = temperature,
            Messages =
            [
                new ChatMessage { Role = "system", Content = systemPrompt },
                new ChatMessage { Role = "user", Content = userPrompt }
            ]
        };

        var response = await _http.PostAsJsonAsync("/v1/chat/completions", request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<ChatResponse>();
        return json?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? "";
    }

    public void Dispose() => _http.Dispose();

    private sealed class ChatRequest
    {
        [JsonPropertyName("model")] public string Model { get; set; } = "";
        [JsonPropertyName("messages")] public List<ChatMessage> Messages { get; set; } = [];
        [JsonPropertyName("temperature")] public double Temperature { get; set; }
    }

    private sealed class ChatMessage
    {
        [JsonPropertyName("role")] public string Role { get; set; } = "";
        [JsonPropertyName("content")] public string Content { get; set; } = "";
    }

    private sealed class ChatResponse
    {
        [JsonPropertyName("choices")] public List<ChatChoice>? Choices { get; set; }
    }

    private sealed class ChatChoice
    {
        [JsonPropertyName("message")] public ChatMessage? Message { get; set; }
    }
}
