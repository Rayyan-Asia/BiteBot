using System.Text.Json.Serialization;

namespace BiteBot.Models;

public class OllamaResponse
{
    [JsonPropertyName("response")]
    public string? Response { get; init; }

    [JsonPropertyName("model")]
    public string? Model { get; init; }
        
    [JsonPropertyName("done")]
    public bool Done { get; init; }
}