using Chirp.Domain;
using Chirp.Infrastructure;

namespace Chirp.Tests.Mock_Stub_Classes;

public class UserServiceFake : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserServiceFake(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public Task<User?> FindByIdAsync(string id)
    {
        // Since UserRepositoryFake doesn't have FindById, we need to check by name or email
        // You might need to add this method to IUserRepository
        throw new NotImplementedException("FindByIdAsync not implemented in UserRepositoryFake");
    }

    public Task<User?> FindByNameAsync(string name)
    {
        return _userRepository.findUserByName(name);
    }

    public Task<User?> FindByEmailAsync(string email)
    {
        return _userRepository.findUserByEmail(email);
    }

    public Task<string> FollowAsync(User follower, string followeeId)
    {
        return _userRepository.followUser(follower, followeeId);
    }

    public Task<string> UnfollowAsync(User follower, string followeeId)
    {
        return _userRepository.UnfollowUser(follower, followeeId);
    }

    public Task<List<User>> GetFollowersAsync(User user)
    {
        return _userRepository.getFollowers(user);
    }

    public Task<List<string>> GetFollowingsAsync(User user)
    {
        return _userRepository.getFollowings(user);
    }
}