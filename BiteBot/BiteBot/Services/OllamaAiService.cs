using System.Text;
using System.Text.Json;
using BiteBot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BiteBot.Services;

public class OllamaAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaAiService> _logger;
    private readonly string _ollamaUrl;
    private readonly string _model;

    public OllamaAiService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<OllamaAiService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
        _ollamaUrl = configuration["OllamaUrl"] ?? "http://localhost:11434";
        _model = configuration["OllamaModel"] ?? "llama3.1";
        _httpClient.Timeout = TimeSpan.FromMinutes(5); // Ollama can take time to generate responses
    }

    public async Task<string> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending prompt to Ollama AI model {Model}", _model);

            var requestContent = CreateRequestContent(prompt);
            var response = await SendRequestToOllamaAsync(requestContent, cancellationToken);
            var ollamaResponse = await ParseResponseAsync(response, cancellationToken);
            
            return ValidateAndExtractResponse(ollamaResponse);
        }
        catch (HttpRequestException ex)
        {
            return HandleHttpRequestException(ex);
        }
        catch (TaskCanceledException ex)
        {
            return HandleTimeoutException(ex);
        }
        catch (Exception ex)
        {
            return HandleUnexpectedException(ex);
        }
    }

    private StringContent CreateRequestContent(string prompt)
    {
        var requestBody = new OllamaRequest
        {
            Model = _model,
            Prompt = prompt,
            Stream = false
        };

        var json = JsonSerializer.Serialize(requestBody);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private async Task<HttpResponseMessage> SendRequestToOllamaAsync(
        StringContent content, 
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsync(
            $"{_ollamaUrl}/api/generate",
            content,
            cancellationToken);

        response.EnsureSuccessStatusCode();
        return response;
    }

    private async Task<OllamaResponse?> ParseResponseAsync(
        HttpResponseMessage response, 
        CancellationToken cancellationToken)
    {
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<OllamaResponse>(responseContent);
    }

    private string ValidateAndExtractResponse(OllamaResponse? ollamaResponse)
    {
        if (ollamaResponse?.Response == null)
        {
            _logger.LogWarning("Ollama returned empty response");
            return "I couldn't generate a summary at this time.";
        }

        _logger.LogInformation("Successfully received response from Ollama");
        return ollamaResponse.Response;
    }

    private string HandleHttpRequestException(HttpRequestException ex)
    {
        _logger.LogError(ex, "HTTP error while communicating with Ollama at {Url}", _ollamaUrl);
        throw new Exception($"Failed to communicate with AI service: {ex.Message}", ex);
    }

    private string HandleTimeoutException(TaskCanceledException ex)
    {
        _logger.LogError(ex, "Request to Ollama timed out");
        throw new Exception("AI service request timed out. Please try again.", ex);
    }

    private string HandleUnexpectedException(Exception ex)
    {
        _logger.LogError(ex, "Unexpected error while communicating with Ollama");
        throw new Exception($"An unexpected error occurred: {ex.Message}", ex);
    }
}