namespace Chirp.Domain;

/// <summary>
/// A data transfer object for cheeps.
/// </summary>
public class CheepDTO
{
    public  int CheepId { get; set; }
    public required string Text { get; set; }
    public required User User { get; set; }
    public List<string>? Likes { get; set; }
    public DateTime TimeStamp { get; set; }
}
