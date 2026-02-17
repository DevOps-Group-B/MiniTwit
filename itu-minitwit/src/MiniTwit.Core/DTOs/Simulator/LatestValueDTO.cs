using System.Text.Json.Serialization;

namespace Chirp.Core.DTOs.Simulator;

public class LatestValueDTO
{
    [JsonPropertyName("latest")]
    public int Latest { get; set; }
}