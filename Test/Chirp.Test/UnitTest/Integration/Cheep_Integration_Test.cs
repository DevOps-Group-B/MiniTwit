using System.Text;
using Chirp.Tests.Infrastructure;
using Chirp.Infrastructure;
using Chirp.Domain;
using Chirp.Tests.Tools_to_Test;
using Microsoft.AspNetCore.Identity;
using Xunit;
[Collection("sqlite-db")]

public class Cheep_Integration_Test
{
    private readonly CheepDbContext _context;
    private readonly CheepRepository _cheepRepository;
    private readonly UserRepository _userRepository;
    private readonly CheepService _CheepService;
    private readonly UserService _UserService;
    private readonly SqliteInMemoryDbFixture _fixture;

    public Cheep_Integration_Test(SqliteInMemoryDbFixture fixture)
    {
        _fixture = fixture;
        _context = fixture.CreateContext();
        _cheepRepository = new CheepRepository(_context);
        _userRepository = new UserRepository(_context);
        _UserService = new UserService(_userRepository);
        _CheepService = new CheepService(_cheepRepository, _UserService);
    }
    
    [Fact]
    public async Task SeedDatabaseTest()
    {
        _fixture.ResetDatabase();
        
        Assert.Empty(_context.Cheeps);
        
        DbInitializer.SeedDatabase(_context);
        
        Assert.NotEmpty(_context.Cheeps);
        Assert.True(_context.Users.Any(u => u.UserName == "Jacqualine Gilcoine"));
        await Task.CompletedTask;
    }
    
    [Fact]
    public async Task Get_Cheeps_From_Author_Is_Usable()
    {
        
        var testUser = HelperClasses.createRandomUser();
        _context.Users.Add(testUser);
        await _context.SaveChangesAsync();

        var cheep = HelperClasses.createRandomCheep(testUser);
        _context.Cheeps.Add(cheep);
        await _context.SaveChangesAsync(); 
        
        var Cheeps = await _CheepService.getCheepsFromUser(testUser, 0);
        Assert.NotNull(Cheeps);
        Assert.NotEmpty(Cheeps);
        Assert.Equal(cheep.Text, Cheeps[0].Text);
    }
    
    
  [Fact]
    public async Task GetCheepsFromUserId_ReturnsCorrectCheepsForUser()
    {
        var user = HelperClasses.createRandomUser();
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        
        var cheep1 = HelperClasses.createRandomCheep(user);
        var cheep2 = HelperClasses.createRandomCheep(user);
        
        var otherUser = HelperClasses.createRandomUser();
        await _context.Users.AddAsync(otherUser);
        await _context.SaveChangesAsync();
        
        var otherCheep = HelperClasses.createRandomCheep(otherUser);
        
        await _context.Cheeps.AddAsync(cheep1);
        await _context.Cheeps.AddAsync(cheep2);
        await _context.Cheeps.AddAsync(otherCheep);
        await _context.SaveChangesAsync();
        
        var result = await _cheepRepository.getCheepsFromUserId(user.Id, 1);
        var result2 = await _CheepService.GetCheepsFromUserId(user.Id, 1);  
        
        Assert.NotNull(result);
        Assert.NotNull(result2);
        
        Assert.Equal(2, result.Count);
        Assert.Equal(result.Count, result2.Count);
        Assert.All(result, cheep => Assert.Equal(user.Id, cheep.User.Id));
        Assert.True(result[0].TimeStamp >= result[1].TimeStamp);
    }
    
    [Fact]
    public async Task GetCheepsFromUserId_ReturnsEmptyListWhenUserHasNoCheeps()
    {
        var userId = "some-nonexistent-user-id";
        
        var result = await _cheepRepository.getCheepsFromUserId(userId, 1);
        
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCheepsFromUserId_ReturnsEmptyListForNonExistentUserId()
    {
        var nonExistentUserId = "non-existent-user-id";
        var result = await _cheepRepository.getCheepsFromUserId(nonExistentUserId, 1);
        
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCheepsFromUserId_PageNumberLessThanOne_ReturnsFirstPage()
    {
        
        var user = HelperClasses.createRandomUser();
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        
        var cheep1 = HelperClasses.createRandomCheep(user);
        var cheep2 = HelperClasses.createRandomCheep(user);
        
        await _context.Cheeps.AddAsync(cheep1);
        await _context.Cheeps.AddAsync(cheep2);
        await _context.SaveChangesAsync();
        
        var result = await _cheepRepository.getCheepsFromUserId(user.Id, 0);
        
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetCheepsFromUserId_IncludesUserInformation()
    {
        
        var user = HelperClasses.createRandomUser();
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        
        var cheep = HelperClasses.createRandomCheep(user);
        await _context.Cheeps.AddAsync(cheep);
        await _context.SaveChangesAsync();
        
        var result = await _cheepRepository.getCheepsFromUserId(user.Id, 1);
        
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
 
        await _CheepService.InsertCheepAsync(cheep);
 
       
        var cheeps = await _CheepService.getCheepsFromUser(testUser, 0);
        var cheepId = cheeps[0].CheepId;
 
        var result = await _CheepService.LikeCheep(testUser, cheepId);
        Assert.Equal("Success", result);
     
    }
    
    [Fact]
    public async Task UnLikeAPostTest()
    {
       
        var testUser = HelperClasses.createRandomUser();
        var cheep = HelperClasses.createRandomCheepDTO(testUser);
 
        await _CheepService.InsertCheepAsync(cheep);
        
        var cheeps = await _CheepService.getCheepsFromUser(testUser, 0);
        var cheepId = cheeps[0].CheepId;
 
        var result = await _CheepService.LikeCheep(testUser, cheepId);
        Assert.Equal("Success", result);
     
        var result2 = await _CheepService.UnLikeCheep(testUser, cheepId);
        Assert.Equal("Success", result2);
    }
    
    [Fact] 
    public async Task ReadCheeps()
    {
       
        var testUser = HelperClasses.createRandomUser();
        var cheep = HelperClasses.createRandomCheepDTO(testUser);
        var cheep2 = HelperClasses.createRandomCheepDTO(testUser);

        await _CheepService.InsertCheepAsync(cheep);
        await _CheepService.InsertCheepAsync(cheep2);


        var result3 = await _CheepService.GetCheepsAsync(1);
        Assert.NotNull(result3);

       
    }
    
}
