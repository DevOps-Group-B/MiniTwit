using System.ComponentModel.DataAnnotations;
    
namespace Chirp.Core.DTOs;

/// <summary>
/// AuthorDTO encapsulates data and minimizes unnecessary data exposure.
/// </summary>
public class AuthorDTO
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public string? Bio { get; set; }
}