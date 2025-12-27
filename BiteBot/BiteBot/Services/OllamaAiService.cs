using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BiteBot.Services;

public class OllamaAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaAiService> _logger;
    private readonly string _ollamaUrl;
    private const string Model = "llama3.1";

    public OllamaAiService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<OllamaAiService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
        _ollamaUrl = configuration["OllamaUrl"] ?? "http://localhost:11434";
        _httpClient.Timeout = TimeSpan.FromMinutes(5); // Ollama can take time to generate responses
    }

    public async Task<string> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending prompt to Ollama AI model {Model}", Model);

            var requestBody = new OllamaRequest
            {
                Model = Model,
                Prompt = prompt,
                Stream = false
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"{_ollamaUrl}/api/generate",
                content,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseContent);

            if (ollamaResponse?.Response == null)
            {
                _logger.LogWarning("Ollama returned empty response");
                return "I couldn't generate a summary at this time.";
            }

            _logger.LogInformation("Successfully received response from Ollama");
            return ollamaResponse.Response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while communicating with Ollama at {Url}", _ollamaUrl);
            throw new Exception($"Failed to communicate with AI service: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request to Ollama timed out");
            throw new Exception("AI service request timed out. Please try again.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while communicating with Ollama");
            throw new Exception($"An unexpected error occurred: {ex.Message}", ex);
        }
    }

    private class OllamaResponse
    {
        [JsonPropertyName("response")]
        public string? Response { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }
        
        [JsonPropertyName("done")]
        public bool Done { get; set; }
    }

    private class OllamaRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }
        [JsonPropertyName("prompt")]
        public string Prompt { get; set; }
        [JsonPropertyName("stream")]
        public bool Stream { get; set; }
    }
}

