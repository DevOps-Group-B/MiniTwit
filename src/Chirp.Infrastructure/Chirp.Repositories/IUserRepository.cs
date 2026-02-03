using Chirp.Domain;

namespace Chirp.Infrastructure;

public interface IUserRepository
{
    Task<User?> findUserByName(string name);

    Task<User?> findUserByEmail(string email);

    Task<List<User>> getFollowers(User user);

    Task<List<string>> getFollowings(User user);
    Task<String> followUser(User user, string followeeID);
    Task<String> UnfollowUser(User user, string followeeID);


}