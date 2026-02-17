using System.ComponentModel.DataAnnotations;

namespace Chirp.Core.Models;

public class AuthorFollower
{
    [Required]
    public required string FollowerId { get; set; }

    public Author Follower { get; set; } = null!;
    
    [Required]
    public required string FollowingId { get; set; } 
    
    public Author Following { get; set; } = null!;
}