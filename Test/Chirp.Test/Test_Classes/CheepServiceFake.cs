using Chirp.Domain;
using Chirp.Infrastructure;

namespace Chirp.Tests.Mock_Stub_Classes;

public class CheepServiceFake : ICheepService
{
    private readonly ICheepRepository _cheepRepository;
    private readonly IUserService _userService;

    public CheepServiceFake(ICheepRepository cheepRepository, IUserService userService)
    {
        _cheepRepository = cheepRepository;
        _userService = userService;
    }

    public Task<List<CheepDTO>> GetCheepsAsync(int page)
    {
        return _cheepRepository.ReadCheeps(page);
    }

    public async Task InsertCheepAsync(CheepDTO cheep)
    {
        await _cheepRepository.InsertNewCheepAsync(cheep);
    }

    public Task<User?> findUserByEmail(string email)
    {
        return _userService.FindByEmailAsync(email);
    }

    public Task<User?> findUserByName(string name)
    {
        return _userService.FindByNameAsync(name);
    }

    public Task<List<CheepDTO>> getCheepsFromUser(User user, int page)
    {
        return _cheepRepository.getCheepsFromUser(user, page);
    }

    public async Task<User?> FindTimelineByUserNameAsync(string userName)
    {
        return await _userService.FindByNameAsync(userName);
    }

    public Task<List<User>> getFollowers(User user)
    {
        return _userService.GetFollowersAsync(user);
    }

    public Task<List<string>> getFollowings(User user)
    {
        return _userService.GetFollowingsAsync(user);
    }

    public Task<string> followUser(User user, string followeeID)
    {
        return _userService.FollowAsync(user, followeeID);
    }

    public Task<string> UnfollowUser(User user, string followeeID)
    {
        return _userService.UnfollowAsync(user, followeeID);
    }

    public async Task<List<CheepDTO>?> GetCheepsFromUserId(string userId, int PageNumber)
    {
        var cheeps = await _cheepRepository.getCheepsFromUserId(userId, PageNumber);
        return cheeps ?? null; 
    }

    public Task<string> LikeCheep(User currentUser, int cheepId)
    {
        return _cheepRepository.LikeCheep(currentUser, cheepId);
    }

    public Task<string> UnLikeCheep(User currentUser, int cheepId)
    {
        return _cheepRepository.UnLikeCheep(currentUser, cheepId);
    }
}