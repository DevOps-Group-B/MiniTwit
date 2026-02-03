using Chirp.Domain;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure;

public class UserRepository : IUserRepository
{
    private readonly CheepDbContext _dbContext;

    public UserRepository(CheepDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    /// <summary>
    /// Finds the first <see cref="User"/> whos name matches the specified name. 
    /// </summary>
    /// <param name="name">The username to search for.</param>
    /// <returns>The matching <see cref="User"/> if found otherwise <c>null</c>.</returns>
    public async Task<User?> findUserByName(string name)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == name);
    }
    
    /// <summary>
    /// Finds the <see cref="User"/> whos email matches the specified email. 
    /// </summary>
    /// <param name="email">The email to search for.</param>
    /// <returns>The matching <see cref="User"/> if found otherwise <c>null</c>.</returns>
    public async Task<User?> findUserByEmail(string email)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <summary>
    /// Retrieves all users who are following the specified user.
    /// </summary>
    /// <param name="user">The user whose followers to retrieve.</param>
    /// <returns>A list of <see cref="User"/> entities representing the followers.</returns>
    public async Task<List<User>> getFollowers(User user)
    {
        return await _dbContext.Follows
            .Where(f => f.FolloweeId == user.Id)
            .Select(f => f.Follower)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves the IDs of all users that the specified user follows.
    /// </summary>
    /// <param name="user">The user whose followings to retrieve.</param>
    /// <returns>A list of user IDs representing the users that the specified user follows.</returns>
    public async Task<List<string>> getFollowings(User user)
    {
        var followers = await _dbContext.Follows
            .Where(f => f.FollowerId == user.Id)
            .Select(f => f.FolloweeId)
            .ToListAsync();

        return followers;
    }
    
    /// <summary>
    /// Creates a follow relationship between the specified user and another user.
    /// </summary>
    /// <param name="user">The user who wants to follow another user.</param>
    /// <param name="followeeID">The ID of the user to be followed.</param>
    /// <returns>Returns a string indicating whether the follow operation was successful</returns>
    public async Task<String> followUser(User user, string followeeID)
    {
        var follow = new Follow
        {
            FollowerId = user.Id,
            FolloweeId = followeeID,
            FollowedAt = DateTime.UtcNow
        };

        _dbContext.Follows.Add(follow);
        await _dbContext.SaveChangesAsync();

        //Check if the follow was successful
        var followers = await _dbContext.Follows
            .Where(f => f.FollowerId == user.Id)
            .Select(f => f.FolloweeId)
            .ToListAsync();
        if (followers.Contains(followeeID))
        {
            return "successfully followed";
        }
        else
        {
            return "failure to follow user";
        }
    }
    
    /// <summary>
    /// Removes a follow relationship between the specified user and another user.
    /// </summary>
    /// <param name="user">The user who wants to unfollow another user.</param>
    /// <param name="followeeID">The ID of the user to be unfollowed.</param>
    /// <returns>Returns a string indicating whether the unfollow operation was successful</returns>
    public async Task<String> UnfollowUser(User user, string followeeID)
    {
        var follow = await _dbContext.Follows.FirstOrDefaultAsync(f => f.FollowerId == user.Id && f.FolloweeId == followeeID);

        if (follow == null)
        {
            // If no follow relationship exists, return failure
            return "failure to unfollow user";
        } 
        _dbContext.Remove(follow);
        await _dbContext.SaveChangesAsync();

        //Check if the Unfollow was successful
        var followers = await _dbContext.Follows
            .Where(f => f.FollowerId == user.Id)
            .Select(f => f.FolloweeId)
            .ToListAsync();

        if (!followers.Contains(followeeID))
        {
            return "successfully unfollowed";
        }
        else
        {
            return "failure to unfollow user";
        }
    }
}