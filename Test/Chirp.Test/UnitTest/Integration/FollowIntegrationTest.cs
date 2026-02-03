using FluentAssertions;
using Chirp.Domain;
using Chirp.Infrastructure;
using Chirp.Tests.Infrastructure;
using Chirp.Tests.Tools_to_Test;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

[Collection("sqlite-db")]

public class FollowIntegrationTests
{
    private readonly CheepDbContext _context;
    private readonly UserRepository _userRepository;
    private readonly UserService _userService;
    private readonly SqliteInMemoryDbFixture _fixture;
  
    public FollowIntegrationTests(SqliteInMemoryDbFixture fixture)
    {
        _fixture = fixture;
        _context = fixture.CreateContext();
        _userRepository = new UserRepository(_context);
        
        // Create a simple mock for UserManager using Moq
        // Looks a bit wild, but this is needed to prevent build warnings.
        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<IPasswordHasher<User>>(),
            Array.Empty<IUserValidator<User>>(),
            Array.Empty<IPasswordValidator<User>>(),
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<User>>>()
        );
        
        userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => 
                _context.Users.FirstOrDefault(u => u.Id == id));
        
        _userService = new UserService(_userRepository, userManagerMock.Object);
    }



    private async Task<(User follower, User followee)> CreateUsersAsync()
    {
        var follower = HelperClasses.createRandomUser();
        var followee = HelperClasses.createRandomUser();

        _context.Users.Add(follower);
        _context.Users.Add(followee);
        await _context.SaveChangesAsync();

        await _context.Entry(follower).ReloadAsync();
        await _context.Entry(followee).ReloadAsync();

        return (follower, followee);
    }

    [Fact]
    public async Task FollowAUser()
    {
        _fixture.ResetDatabase();
        
        var (follower, followee) = await CreateUsersAsync();
        var result = await _userService.FollowAsync(follower, followee.Id);

        result.Should().NotBeNull();
    }
    
    [Fact]
    public async Task FollowAUserAndGetItOnFollowlist()
    {
        _fixture.ResetDatabase();

        var (follower, followee) = await CreateUsersAsync();

        // Use UserService for follow
        var result = await _userService.FollowAsync(follower, followee.Id);
        result.Should().NotBeNull();

        // Use UserService to get followings
        var followedUsers = await _userService.GetFollowingsAsync(follower);

        followedUsers.Should().Contain(followee.Id);
    }

    [Fact]
    public async Task UnFollowAUser()
    {
        _fixture.ResetDatabase();

        var (follower, followee) = await CreateUsersAsync();

        // Follow first using UserService
        var followResult = await _userService.FollowAsync(follower, followee.Id);
        followResult.Should().NotBeNull();

        // Then unfollow using UserService
        var unfollowResult = await _userService.UnfollowAsync(follower, followee.Id);

        unfollowResult.Should().NotBeNull();
    }

    [Fact]
    public async Task UnFollowAUserAndRemoveItFromFollowlist()
    {
        _fixture.ResetDatabase();
        var (follower, followee) = await CreateUsersAsync();
        
        // Follow using UserService
        var followResult = await _userService.FollowAsync(follower, followee.Id);
        followResult.Should().NotBeNull();

        // Get followings using UserService
        var followedUsersBefore = await _userService.GetFollowingsAsync(follower);
        followedUsersBefore.Should().Contain(followee.Id);

        // Unfollow using UserService
        var unfollowResult = await _userService.UnfollowAsync(follower, followee.Id);
        unfollowResult.Should().NotBeNull();

        // Get followings again using UserService
        var followedUsersAfter = await _userService.GetFollowingsAsync(follower);
        followedUsersAfter.Should().NotContain(followee.Id);
    }
}