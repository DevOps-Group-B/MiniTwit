using System.ComponentModel.DataAnnotations;

namespace Chirp.Core.Models;

public class AuthorAchievement
{
    [Required]
    public required string AuthorId { get; set; }

    public Author Author { get; set; } = null!;
    
    [Required]
    public required int AchievementId { get; set; } 
    
    public Achievement Achievement { get; set; } = null!;
    
    public DateTime AchievedAt { get; set; }
}