using System.Text.Json.Serialization;

namespace Chirp.Core.DTOs.Simulator;

public class PostMessageDTO
{
    [JsonPropertyName("content")]
    public string Content { get; set; }
}