using System.Text.Json.Serialization;

namespace BiteBot.Models;

public class OllamaRequest
{
    [JsonPropertyName("model")]
    public string? Model { get; set; }
    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; }
    [JsonPropertyName("stream")]
    public bool Stream { get; set; }
}