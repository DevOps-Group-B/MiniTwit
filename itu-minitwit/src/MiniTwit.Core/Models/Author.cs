using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Chirp.Core.Models;

public class Author : IdentityUser
{
    [Required]
    public required ICollection<Cheep> Cheeps { get; set; }

    [Required]
    public required string Name { get; set; }
    
    [StringLength(300)]
    public string? Bio { get; set; }
    
    [Required]
    public required ICollection<AuthorFollower> Followers { get; set; }
    
    [Required]
    public required ICollection<AuthorFollower> Following { get; set; }

    [Required] 
    public ICollection<AuthorAchievement> AuthorAchievements { get; set; } = new List<AuthorAchievement>();
}