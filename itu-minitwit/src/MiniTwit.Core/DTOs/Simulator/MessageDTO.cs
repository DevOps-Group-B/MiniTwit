using System.Text.Json.Serialization;

namespace Chirp.Core.DTOs.Simulator;

public class MessageDTO
{
    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonPropertyName("pub_date")]
    public string PubDate { get; set; }

    [JsonPropertyName("user")]
    public string User { get; set; }
}