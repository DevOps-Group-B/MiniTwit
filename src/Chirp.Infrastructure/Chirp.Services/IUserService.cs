using Chirp.Domain;

namespace Chirp.Infrastructure;

public interface IUserService
{
    Task<User?> FindByIdAsync(string id);
    Task<User?> FindByNameAsync(string name);
    Task<User?> FindByEmailAsync(string email);

    Task<string> FollowAsync(User follower, string followeeId);
    Task<string> UnfollowAsync(User follower, string followeeId);

    Task<List<User>> GetFollowersAsync(User user);
    Task<List<string>> GetFollowingsAsync(User user);
}
