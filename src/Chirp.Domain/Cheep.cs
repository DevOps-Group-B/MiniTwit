using System.ComponentModel.DataAnnotations;
namespace Chirp.Domain;

/// <summary>
/// Represents a cheep in the chirp application
/// A cheep is a short message,. containing a maximum of 160 characters. 
/// </summary>
public class Cheep
{
    public int CheepId { get; set; }

    [StringLength(160)]
    public required string Text { get; set; }
    public required string UserId { get; set; }
    public required User User { get; set; }
    public DateTime TimeStamp { get; set; }
    public List<string>? Likes { get; set; }
}
