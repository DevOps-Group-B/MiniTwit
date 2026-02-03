namespace Chirp.Domain;

/// <summary>
/// Represents a user in the chirp application. 
/// </summary>
public class User : Microsoft.AspNetCore.Identity.IdentityUser
{
    public byte[]? ProfilePicture { get; set; }
    public string? Name { get; set; }
    public required ICollection<Cheep> Cheeps { get; set; }
    
}