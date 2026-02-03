using Chirp.Domain;
namespace Chirp.Infrastructure;

public interface ICheepService
{
    Task<List<CheepDTO>> GetCheepsAsync(int page);
    Task InsertCheepAsync(CheepDTO cheep);

    Task<User?> findUserByEmail(string email);
    Task<User?> findUserByName(string name);

    Task<List<CheepDTO>> getCheepsFromUser(User user, int page);

    Task<User?> FindTimelineByUserNameAsync(string userName);

    Task<List<User>> getFollowers(User user);

    Task<List<string>> getFollowings(User user);


    Task<String> followUser(User user, string followeeID);


    Task<String> UnfollowUser(User user, string followeeID);
    Task<List<CheepDTO>?> GetCheepsFromUserId(string userId, int PageNumber);
    Task<string> LikeCheep(User currentUser, int cheepId);
    Task<string> UnLikeCheep(User currentUser, int cheepId);
}