using Chirp.Domain;
using Chirp.Infrastructure;

namespace Chirp.Tests.Mock_Stub_Classes;
public class UserRepositoryFake : IUserRepository
{
    private readonly List<User> _users = new();
    private readonly List<Follow> _follows = new();

    public Task<User?> findUserByName(string name)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.UserName == name));
    }

    public Task<User?> findUserByEmail(string email)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.Email == email));
    }

    public Task<List<User>> getFollowers(User user)
    {
        var followers = _follows
            .Where(f => f.FolloweeId == user.Id)
            .Select(f => f.Follower)
            .ToList();
        return Task.FromResult(followers);
    }

    public Task<List<string>> getFollowings(User user)
    {
        var followings = _follows
            .Where(f => f.FollowerId == user.Id)
            .Select(f => f.FolloweeId)
            .ToList();
        return Task.FromResult(followings);
    }

    public Task<string> followUser(User user, string followeeID)
    {
        _follows.Add(new Follow
        {
            FollowerId = user.Id,
            FolloweeId = followeeID,
            Follower = user,
            FollowedAt = DateTime.UtcNow
        });
        return Task.FromResult("successfully followed");
    }

    public Task<string> UnfollowUser(User user, string followeeID)
    {
        var follow = _follows.FirstOrDefault(f => f.FollowerId == user.Id && f.FolloweeId == followeeID);
        if (follow != null)
        {
            _follows.Remove(follow);
            return Task.FromResult("successfully unfollowed");
        }
        return Task.FromResult("failure to unfollow user");
    }
    
    public void addUser(User user)
    {
        _users.Add(user);
    }
    
}