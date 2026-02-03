using Chirp.Domain;
namespace Chirp.Infrastructure;

// changed to interface for making libskob principle availible (for tests)
// changed to primary constructor as well :^)
/// <summary>
/// Provides application-level operations for managing cheeps.
/// </summary>
public class CheepService(ICheepRepository cheepRepo, IUserService userService) : ICheepService
{

    /// <summary>
    /// Retrieves a paginated list of cheeps, ordered by most recent first.
    /// </summary>
    /// <param name="page">The page number to retrieve.</param>
    /// <returns>A list of <see cref="CheepDTO"/> objects for the specified page.</returns>
    public async Task<List<CheepDTO>> GetCheepsAsync(int page)
    {
        return await cheepRepo.ReadCheeps(page) ?? new List<CheepDTO>();
    }

    /// <summary>
    /// Adds a new cheep to the database.
    /// </summary>
    /// <param name="cheep">The cheep to be inserted, represented as a <see cref="CheepDTO"/>.</param>
    public async Task InsertCheepAsync(CheepDTO cheep)
    {
        await cheepRepo.InsertNewCheepAsync(cheep);
    }

    /// <summary>
    /// Retrieves a user by their email address.
    /// </summary>
    /// <param name="email">The email of the user to find.</param>
    /// <returns>The matching <see cref="User"/> if found, otherwise <c>null</c>.</returns>
    public async Task<User?> findUserByEmail(string email)
    {
        return await userService.FindByEmailAsync(email);
    }

    /// <summary>
    /// Retrieves a user by their username.
    /// </summary>
    /// <param name="name">The username of the user to find.</param>
    /// <returns>The matching <see cref="User"/> if found, otherwise <c>null</c>.</returns>
    public async Task<User?> findUserByName(string name)
    {
        return await userService.FindByNameAsync(name);
    }

    /// <summary>
    /// Retrieves a paginated list of cheeps written by a specific user.
    /// </summary>
    /// <param name="user">The user whose cheeps to retrieve.</param>
    /// <param name="page">The page number to retrieve.</param>
    /// <returns>A list of <see cref="CheepDTO"/> objects authored by the specified user.</returns>
    public async Task<List<CheepDTO>> getCheepsFromUser(User user, int page)
    {
        return await cheepRepo.getCheepsFromUser(user, page);
    }

    /// <summary>
    /// Retrieves a list of users who follow the specified user.
    /// </summary>
    /// <param name="user">The user whose followers to retrieve.</param>
    /// <returns>A list of <see cref="User"/> objects representing the followers.</returns>
    public async Task<List<User>> getFollowers(User user)
    {
        return await userService.GetFollowersAsync(user);
    }

    /// <summary>
    /// Retrieves the IDs of all users that the specified user follows.
    /// </summary>
    /// <param name="user">The user whose followings to retrieve.</param>
    /// <returns>A list of user IDs representing the followed users.</returns>
    public async Task<List<string>> getFollowings(User user)
    {
        return await userService.GetFollowingsAsync(user);
    }

    /// <summary>
    /// Follows another user.
    /// </summary>
    /// <param name="user">The user performing the follow action.</param>
    /// <param name="followeeID">The ID of the user to follow.</param>
    /// <returns>A string indicating whether the follow operation was successful.</returns>
    public async Task<String> followUser(User user, string followeeID)
    {
        return await userService.FollowAsync(user, followeeID);
    }

    /// <summary>
    /// Unfollows a previously followed user.
    /// </summary>
    /// <param name="user">The user performing the unfollow action.</param>
    /// <param name="followeeID">The ID of the user to unfollow.</param>
    /// <returns>A string indicating whether the unfollow operation was successful.</returns>
    public async Task<String> UnfollowUser(User user, string followeeID)
    {
        return await userService.UnfollowAsync(user, followeeID);
    }

    public Task<User?> FindTimelineByUserNameAsync(string userName)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieves a paginated list of cheeps written by a specific user ID.
    /// </summary>
    /// <param name="userId">The ID of the user whose cheeps to retrieve.</param>
    /// <param name="page"></param>
    /// <param name="pageNumber">The page number to retrieve.</param>
    /// <returns>A list of <see cref="CheepDTO"/> objects authored by the user.</returns>
    public async Task<List<CheepDTO>?> GetCheepsFromUserId(string userId, int page)
    {
        return await cheepRepo.getCheepsFromUserId(userId, page) ?? new List<CheepDTO>();
    }
    
    /// <summary>
    /// Adds a like to a cheep from the specified user.
    /// </summary>
    /// <param name="currentUser">The user liking the cheep.</param>
    /// <param name="cheepId">The ID of the cheep to like.</param>
    /// <returns>A string indicating whether the operation was successful.</returns>
    public async Task<string> LikeCheep(User currentUser, int cheepId)
    {
        return await cheepRepo.LikeCheep(currentUser, cheepId);
    }
    
    /// <summary>
    /// Removes a like from a cheep by the specified user.
    /// </summary>
    /// <param name="currentUser">The user unliking the cheep.</param>
    /// <param name="cheepId">The ID of the cheep to unlike.</param>
    /// <returns>A string indicating whether the operation was successful.</returns>
    public async Task<string> UnLikeCheep(User currentUser, int cheepId)
    {
        return await cheepRepo.UnLikeCheep(currentUser,cheepId);
    }
}

