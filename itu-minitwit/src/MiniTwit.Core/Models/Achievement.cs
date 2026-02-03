using System.ComponentModel.DataAnnotations;

namespace Chirp.Core.Models;

public class Achievement
{
    [Required]
    public int AchievementId { get; set; }
    
    [Required]
    [StringLength(25)]
    public required string Title { get; set; }
    
    [Required]
    [StringLength(100)]
    public required string Description { get; set; }
    
    [Required]
    public required string ImagePath { get; set; }

    [Required] 
    public ICollection<AuthorAchievement> AuthorAchievements { get; set; } = new List<AuthorAchievement>();
}