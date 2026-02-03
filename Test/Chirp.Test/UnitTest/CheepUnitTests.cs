using Chirp.Domain;
using Chirp.Infrastructure;
using Chirp.Tests.Infrastructure;
using Chirp.Tests.Mock_Stub_Classes;
using Chirp.Tests.Tools_to_Test;

namespace Chirp.Tests.UnitTest;

[Collection("sqlite-db")]
public class CheepServiceTests
{
 

    //real for testing
    
    
    //private readonly CheepService _service;
   // private readonly CheepRepository _cheepRepo;
    
    // fake for service methods
    
    
    private readonly CheepServiceFake _CheepServicefake;
    
   // private readonly UserRepositoryFake _userRepo;
   //private readonly SqliteInMemoryDbFixture _fixture;
   private readonly CheepRepository _cheepRepository;
    private readonly CheepDbContext _context;

    public CheepServiceTests(SqliteInMemoryDbFixture fixture)
    {
        _context = fixture.CreateContext(); 
        _cheepRepository = new CheepRepository(_context);

        // ... existing fake setup code remains
        var userRepoFake = new UserRepositoryFake();
        var cheepRepo = new CheepRepository(_context);
        var userServiceFake = new UserServiceFake(userRepoFake); 
        
        _CheepServicefake = new CheepServiceFake(cheepRepo, userServiceFake);

        
        
    }

    [Fact]
    public async Task GetCheepsFromUserId_ReturnsCorrectCheepsForUser()
    {
        // Arrange
        // Create a user and add to database
        var user = HelperClasses.createRandomUser();
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        
        // Create cheeps for this user
        var cheep1 = new Cheep 
        { 
            Text = "Test cheep 1",
            UserId = user.Id,
            User = user,
            TimeStamp = DateTime.Now.AddMinutes(-10)
        };
        
        var cheep2 = new Cheep 
        { 
            Text = "Test cheep 2",
            UserId = user.Id,
            User = user,
            TimeStamp = DateTime.Now.AddMinutes(-5)
        };
        
        // Create another user with different cheeps
        var otherUser = HelperClasses.createRandomUser();
        await _context.Users.AddAsync(otherUser);
        await _context.SaveChangesAsync();
        
        var otherCheep = new Cheep 
        { 
            Text = "Other user's cheep",
            UserId = otherUser.Id,
            User = otherUser,
            TimeStamp = DateTime.Now
        };
        
        // Insert all cheeps
        await _context.Cheeps.AddAsync(cheep1);
        await _context.Cheeps.AddAsync(cheep2);
        await _context.Cheeps.AddAsync(otherCheep);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _cheepRepository.getCheepsFromUserId(user.Id, 1);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, cheep => Assert.Equal(user.Id, cheep.User.Id));
        // Should be ordered by timestamp descending (newest first)
        Assert.True(result[0].TimeStamp >= result[1].TimeStamp);
    }
    
    
    // inmemory database setup wrong no user tables,
    [Fact(Skip = "Missing AspNetUsers table. Fix database setup.")]
    public async Task GetCheepsFromUserId_ReturnsEmptyListWhenUserHasNoCheepssimple()
    {
       
    
        // Arrange
        var userId = "some-nonexistent-user-id";
    
        // Act
        var result = await _cheepRepository.getCheepsFromUserId(userId, 1);
    
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    [Fact(Skip = "Missing AspNetUsers table. Fix database setup.")]
    
    public async Task GetCheepsFromUserId_ReturnsEmptyListWhenUserHasNoCheeps()
    {
        // Arrange
        var user = HelperClasses.createRandomUser(); // This user is not saved to the database.
        
        // Create another user with cheeps to ensure we're filtering correctly
        var otherUser = HelperClasses.createRandomUser();
        await _context.Users.AddAsync(otherUser);
        await _context.SaveChangesAsync();
        
        var otherCheep = new Cheep 
        { 
            Text = "Other user's cheep",
            UserId = otherUser.Id,
            User = otherUser,
            TimeStamp = DateTime.Now
        };
        await _context.Cheeps.AddAsync(otherCheep);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _cheepRepository.getCheepsFromUserId(user.Id, 1);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCheepsFromUserId_ReturnsEmptyListForNonExistentUserId()
    {
        // Arrange
        var nonExistentUserId = "non-existent-user-id";
        
        // Act
        var result = await _cheepRepository.getCheepsFromUserId(nonExistentUserId, 1);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCheepsFromUserId_PageNumberLessThanOne_ReturnsFirstPage()
    {
        // Arrange
        var user = HelperClasses.createRandomUser();
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        
        // Create a few cheeps
        var cheep1 = new Cheep 
        { 
            Text = "Test cheep 1",
            UserId = user.Id,
            User = user,
            TimeStamp = DateTime.Now.AddMinutes(-10)
        };
        
        var cheep2 = new Cheep 
        { 
            Text = "Test cheep 2",
            UserId = user.Id,
            User = user,
            TimeStamp = DateTime.Now.AddMinutes(-5)
        };
        
        await _context.Cheeps.AddAsync(cheep1);
        await _context.Cheeps.AddAsync(cheep2);
        await _context.SaveChangesAsync();
        
        // Act - page number 0 should be treated as page 1
        var result = await _cheepRepository.getCheepsFromUserId(user.Id, 0);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetCheepsFromUserId_IncludesUserInformation()
    {
        // Arrange
        var user = HelperClasses.createRandomUser();
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        
        var cheep = new Cheep 
        { 
            Text = "Test cheep",
            UserId = user.Id,
            User = user,
            TimeStamp = DateTime.Now
        };
        await _context.Cheeps.AddAsync(cheep);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _cheepRepository.getCheepsFromUserId(user.Id, 1);
        
        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.NotNull(result[0].User);
        Assert.Equal(user.Id, result[0].User.Id);
        Assert.Equal(user.UserName, result[0].User.UserName);
        Assert.Equal(user.Email, result[0].User.Email);
    }
    
    [Fact]
    public async Task LikeAPostTest()
    {
       
        var testUser = HelperClasses.createRandomUser();
        var cheep = HelperClasses.createRandomCheepDTO(testUser);
 
        await _CheepServicefake.InsertCheepAsync(cheep);
 
       
        var cheeps = await _CheepServicefake.getCheepsFromUser(testUser, 0);
        var cheepId = cheeps[0].CheepId;
 
        var result = await _CheepServicefake.LikeCheep(testUser, cheepId);
        Assert.Equal("Success", result);
     
    }
    
    [Fact]
    public async Task UnLikeAPostTest()
    {
       
        var testUser = HelperClasses.createRandomUser();
        var cheep = HelperClasses.createRandomCheepDTO(testUser);
 
        await _CheepServicefake.InsertCheepAsync(cheep);
 
       
        var cheeps = await _CheepServicefake.getCheepsFromUser(testUser, 0);
        var cheepId = cheeps[0].CheepId;
 
        var result = await _CheepServicefake.LikeCheep(testUser, cheepId);
        Assert.Equal("Success", result);
     
        var result2 = await _CheepServicefake.UnLikeCheep(testUser, cheepId);
        Assert.Equal("Success", result2);
    }
    
    [Fact] 
    public async Task ReadCheeps()
    {
       
        var testUser = HelperClasses.createRandomUser();
        var cheep = HelperClasses.createRandomCheepDTO(testUser);
        var cheep2 = HelperClasses.createRandomCheepDTO(testUser);

        await _CheepServicefake.InsertCheepAsync(cheep);
        await _CheepServicefake.InsertCheepAsync(cheep2);


       var result3 = await _CheepServicefake.GetCheepsAsync(1);
       Assert.NotNull(result3);

       
    }
}