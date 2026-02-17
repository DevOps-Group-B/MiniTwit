using System.Text.Json.Serialization;

namespace Chirp.Core.DTOs.Simulator;

public class ErrorResponseDTO
{
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("error_msg")]
    public string ErrorMsg { get; set; }
}