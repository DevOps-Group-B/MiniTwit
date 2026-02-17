using System.Text.Json.Serialization;

namespace Chirp.Core.DTOs.Simulator;

public class FollowActionDTO
{
    [JsonPropertyName("follow")]
    public string? Follow { get; set; }

    [JsonPropertyName("unfollow")]
    public string? Unfollow { get; set; }
}