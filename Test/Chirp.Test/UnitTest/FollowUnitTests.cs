
using System.Reflection;
using Chirp.Domain;
using Chirp.Infrastructure;

using Chirp.Tests.Mock_Stub_Classes;



public class FollowUnitTests
{
    private readonly ICheepService _service;

    public List<string> followedUsers { get; set; } = new();
    //private readonly CheepService? _CheepServicefake;
    private readonly CheepRepositoryFake _CheepRepoFake;
    private readonly UserRepositoryFake _userRepoFake;
    private readonly UserServiceFake _userServiceFake;
    
    public FollowUnitTests()
    { 
        _userRepoFake = new UserRepositoryFake();
        _CheepRepoFake = new CheepRepositoryFake();
        _userServiceFake = new UserServiceFake(_userRepoFake); 
        _service = new CheepService(_CheepRepoFake,_userServiceFake);
    }


    [Fact]
    public async Task FollowAUser()
    {
        var user = new User { UserName = "validname", Email = "Very_Much_an_email@itu.dk" , Cheeps = new List<Cheep>() };
        var user2 = new User { UserName = "validname2", Email = "Very_Much_an_email2@itu.dk" , Cheeps = new List<Cheep>() };

        
        var result = await _service.followUser(user, user2.Id);

        Assert.True(result is not null);
    }

    [Fact]
    public async Task FollowAUserAndGetItOnFollowlist()
    {
        var user = new User { UserName = "validname", Email = "Very_Much_an_email@itu.dk" , Cheeps = new List<Cheep>() };
        var user2 = new User { UserName = "validname2", Email = "Very_Much_an_email2@itu.dk" , Cheeps = new List<Cheep>() };

        
        var result = await _service.followUser(user, user2.Id);
        
        followedUsers = await _service.getFollowings(user);

        Assert.Contains(user2.Id, followedUsers);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task UnFollowAUser()
    {
        var user = new User { UserName = "validname", Email = "Very_Much_an_email@itu.dk" , Cheeps = new List<Cheep>() };
        var user2 = new User { UserName = "validname2", Email = "Very_Much_an_email2@itu.dk" , Cheeps = new List<Cheep>() };

        
        var result = await _service.followUser(user, user2.Id);
        var result2 = await _service.UnfollowUser(user, user2.Id);

        Assert.True(result is not null);
        Assert.True(result2 is not null);
    }
    [Fact]
    public async Task UnFollowAUserAndRemoveItFromFollowlist()
    {
        var user = new User { UserName = "validname", Email = "Very_Much_an_email@itu.dk" , Cheeps = new List<Cheep>() };
        var user2 = new User { UserName = "validname2", Email = "Very_Much_an_email2@itu.dk" , Cheeps = new List<Cheep>() };

        
        var result = await _service.followUser(user, user2.Id);

        followedUsers = await _service.getFollowings(user);

        Assert.Contains(user2.Id, followedUsers);

        var result2 = await _service.UnfollowUser(user, user2.Id);

        followedUsers = await _service.getFollowings(user);

        Assert.DoesNotContain(user2.Id, followedUsers);
    }

    [Fact]
    public async Task findUserByName()
    {
        var name = "validname";
        var user = new User { UserName = name, Email = "Very_Much_an_email@itu.dk" , Cheeps = new List<Cheep>() };
        
        _userRepoFake.addUser(user);

        var result = await _service.findUserByName(name);

        Assert.Equal(user, result);
    }

    [Fact]
    public async Task findUserByEmail()
    {
        var email = "Very_Much_an_email@itu.dk";
        var user = new User { UserName = "validname", Email = email, Cheeps = new List<Cheep>() };
        
        _userRepoFake.addUser(user);

        var result = await _service.findUserByEmail(email);

        Assert.Equal(user, result);
    }
}