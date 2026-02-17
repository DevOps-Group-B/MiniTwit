using System.Text.Json.Serialization;

namespace Chirp.Core.DTOs.Simulator;

public class RegisterRequestDTO
{
    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("pwd")]
    public string Pwd { get; set; }
}