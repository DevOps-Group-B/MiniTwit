using System.Text.Json.Serialization;

namespace Chirp.Core.DTOs.Simulator;

public class FollowsResponseDTO
{
    [JsonPropertyName("follows")]
    public IList<string> Follows { get; set; } = new List<string>();
}