namespace Chirp.Infrastructure;

using Chirp.Domain;
using Microsoft.AspNetCore.Identity;

/// <summary>
/// Provides high-level user operations such as following, unfollowing, and user lookup.
/// Acts as a bridge between the domain logic and data access layers,
/// using <see cref="IUserRepository"/> for persistence and <see cref="UserManager{TUser}"/> for identity management.
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    private readonly UserManager<User>? _userManager;

    public UserService(IUserRepository users, UserManager<User> userManager)
    {
        _userRepo = users;
        _userManager = userManager;
    }
 
    public UserService(UserRepository userRepository) // for tests
    {
        _userRepo = userRepository;
    }
    
    /// <summary>
    /// Finds a user by their unique identifier using the ASP.NET Identity system.
    /// </summary>
    /// <param name="id">The unique identifier of the user to find.</param>
    /// <returns>The matching <see cref="User"/> if found otherwise <c>null</c>.</returns>
    public Task<User?> FindByIdAsync(string id)
        => _userManager!.FindByIdAsync(id);

    /// <summary>
    /// Finds a user by their username.
    /// </summary>
    /// <param name="name">The username to search for.</param>
    /// <returns>The matching <see cref="User"/> if found otherwise <c>null</c>.</returns>
    public Task<User?> FindByNameAsync(string name)
        => _userRepo.findUserByName(name); // or _userManager.FindByNameAsync(name) if that’s canonical

    /// <summary>
    /// Finds a user by their email address.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <returns>The matching <see cref="User"/> if found otherwise <c>null</c>.</returns>
    public Task<User?> FindByEmailAsync(string email) => _userRepo.findUserByEmail(email); // or _userManager.FindByEmailAsync(email)

    /// <summary>
    /// Creates a follow relationship between two users after validating input and preventing duplicates.
    /// </summary>
    /// <param name="follower">The user initiating the follow action.</param>
    /// <param name="followeeId">The ID of the user to be followed.</param>
    /// <returns>
    /// A string result indicating the outcome.</returns>
    public async Task<string> FollowAsync(User follower, string followeeId)
    {
        if (string.IsNullOrWhiteSpace(followeeId)) return "invalid-followee";
        if (follower.Id == followeeId) return "cannot-follow-self";

        var followee = await FindByIdAsync(followeeId);
        if (followee is null) return "followee-not-found";

        // Prevents duplicates by checking existing followings
        var current = await _userRepo.getFollowings(follower);
        if (current.Contains(followeeId)) return "already-following";

        return await _userRepo.followUser(follower, followeeId);
    }

    /// <summary>
    /// Removes an existing follow relationship between two users.
    /// </summary>
    /// <param name="follower">The user initiating the unfollow action.</param>
    /// <param name="followeeId">The ID of the user to unfollow.</param>
    /// <returns>A string indicating whether the unfollow operation was successful.</returns>
    public Task<string> UnfollowAsync(User follower, string followeeId)
        => _userRepo.UnfollowUser(follower, followeeId);

    /// <summary>
    /// Retrieves all users who are following the specified user.
    /// </summary>
    /// <param name="user">The user whose followers to retrieve.</param>
    /// <returns>A list of <see cref="User"/> entities representing the followers.</returns>
    public Task<List<User>> GetFollowersAsync(User user)
        => _userRepo.getFollowers(user);

    /// <summary>
    /// Retrieves the IDs of all users that the specified user is following.
    /// </summary>
    /// <param name="user">The user whose followings to retrieve.</param>
    /// <returns>A list of user IDs representing the users that the specified user follows.</returns>
    public Task<List<string>> GetFollowingsAsync(User user)
        => _userRepo.getFollowings(user);
}
