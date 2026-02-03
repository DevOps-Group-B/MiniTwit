namespace Chirp.Domain;

/// <summary>
/// Follow is used for tracking who follows whom.
/// </summary>
public class Follow
{
    public required string FollowerId { get; set; }
    public required string FolloweeId { get; set; }
    public required DateTime FollowedAt { get; set; }
   
    // EF Core navigation properties.
    // Not stored in db, but allows EF Core to join Users and Follows tables
    public User Follower { get; set; } = null!;
    public User Followee { get; set; } = null!;
}