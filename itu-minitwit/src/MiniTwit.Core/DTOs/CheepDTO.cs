namespace Chirp.Core.DTOs;

/// <summary>
/// CheepDTO encapsulates data and minimizes unnecessary data exposure.
/// </summary>
public class CheepDTO
{
    public required string AuthorId { get; set; }
    public required string AuthorName { get; set; }
    public required string Message { get; set; }
    public required DateTime Timestamp { get; set; }
}