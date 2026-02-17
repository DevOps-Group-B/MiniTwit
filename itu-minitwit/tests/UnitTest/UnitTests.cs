using Chirp.Core.Models;
using Chirp.Core.Repositories;

namespace unitTest;

using Chirp.Infrastructure.Chirp.Repositories;
using Chirp.Infrastructure.Data;
using Xunit;
using Util;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// UnitTest class is for testing the Chirp application.
/// It tests the AuthorRepository, CheepRepository, and AchievementRepository
/// </summary>
public class UnitTests
{
    public static DateTime CurrentTime = DateTime.Now;


    [Fact]
    public async Task GetAuthorByNameAsync_SingleAuthor_ReturnsAuthor()
    {
        //arrange
        ChirpDBContext context = await Util.CreateInMemoryDatabase();
        IAchievementRepository achievementRepository = new AchievementRepository(context);
        IAuthorRepository authorRepository = new AuthorRepository(context, achievementRepository);

        Author author1 = new Author
        {
            Name = "TestUser1",
            Email = "test1@example.dk",
            Cheeps = new List<Cheep>(),
            Followers = new List<AuthorFollower>(),
            Following = new List<AuthorFollower>()
        };
        context.Authors.AddRange(author1);
        await context.SaveChangesAsync();

        //act
        var authorByName = await authorRepository.GetAuthorByNameAsync(author1.Name);

        //assert
        Assert.Equal(author1.Id, authorByName?.Id);

        //cleanup
        await context.Database.EnsureDeletedAsync();
        await context.DisposeAsync();
    }

    [Fact]
    public async Task GetAuthorByEmailAsync_SingleAuthor_ReturnsAuthor()
    {
        //arrange
        ChirpDBContext context = await Util.CreateInMemoryDatabase();
        IAchievementRepository achievementRepository = new AchievementRepository(context);
        IAuthorRepository authorRepository = new AuthorRepository(context, achievementRepository);
        
        Author author1 = new Author
        {
            Name = "TestUser1",
            Email = "test1@example.dk",
            Cheeps = new List<Cheep>(),
            Followers = new List<AuthorFollower>(),
            Following = new List<AuthorFollower>()
        };
        context.Authors.AddRange(author1);
        await context.SaveChangesAsync();

        //act
        var authorByEmail = await authorRepository.GetAuthorByEmailAsync(author1.Email);

        //assert
        Assert.Equal(author1.Id, authorByEmail?.Id);

        //cleanup
        await context.Database.EnsureDeletedAsync();
        await context.DisposeAsync();
    }

    [Fact]
    public async Task NewAuthorAsync_EmptyDatabase_CreatesNewAuthor()
    {
        //arrange
        ChirpDBContext context = await Util.CreateInMemoryDatabase();
        IAchievementRepository achievementRepository = new AchievementRepository(context);
        IAuthorRepository authorRepository = new AuthorRepository(context, achievementRepository);

        //act
        await authorRepository.NewAuthorAsync("testAuthor", "testAuthor@email.com");
        var authorInDb = context.Authors.First();

        //assert
        Assert.NotNull(authorInDb);
        Assert.Equal("testAuthor@email.com", authorInDb.Email);
        Assert.Equal("testAuthor", authorInDb.Name);
        
        //cleanup
        await context.Database.EnsureDeletedAsync();
        await context.DisposeAsync();
    }

    [Fact]
    public async Task PostCheepAsync_SingleAuthor_CreatesCheepForAuthor()
    {
        //arrange
        ChirpDBContext context = await Util.CreateInMemoryDatabase();
        IAchievementRepository achievementRepository = new AchievementRepository(context);
        ICheepRepository cheepRepository = new CheepRepository(context, achievementRepository);
        
        Achievement ach2 = new Achievement() { AchievementId = 2, Title = "Novice Cheepster", Description = "Congratulations! You created your first Cheep.", ImagePath = "/images/Badges/First-cheep-badge.png" };
        context.Achievements.Add(ach2);
        
        Author author = new Author
        {
            Name = "TestUser1",
            Email = "test1@example.dk",
            Cheeps = new List<Cheep>(),
            Followers = new List<AuthorFollower>(),
            Following = new List<AuthorFollower>()
        };
        context.Authors.AddRange(author);
        
        await context.SaveChangesAsync();
        
        var cheep = new Cheep
        {
            Text = "Hello World!",
            AuthorId = author.Id,
            TimeStamp = CurrentTime
        };

        //act
        await cheepRepository.PostCheepAsync(cheep);

        //assert
        Assert.NotEmpty(author.Cheeps);
        Assert.Equal("Hello World!", author.Cheeps.First().Text);
        
        //cleanup
        await context.Database.EnsureDeletedAsync();
        await context.DisposeAsync();
    }

    [Fact]
    public async Task GetCheepsAsync_40Cheeps_Returns32CheepsOnPage1And8CheepsOnPage2()
    {
        //arrange
        ChirpDBContext context = await Util.CreateInMemoryDatabase();
        var achievementRepo = new AchievementRepository(context);
        var cheepRepo = new CheepRepository(context, achievementRepo);

        var a1 = new Author()
        {
            Name = "Roger Histand",
            Email = "Roger+Histand@hotmail.com",
            Cheeps = new List<Cheep>(),
            Followers = new List<AuthorFollower>(),
            Following = new List<AuthorFollower>()
        };

        context.Authors.Add(a1);

        for (int i = 0; i < 40; i++)
        {
            var cheep = new Cheep
            {
                Text = "" + i,
                AuthorId = a1.Id,
                TimeStamp = CurrentTime,
            };

            context.Cheeps.AddRange(cheep);
        }
        await context.SaveChangesAsync();


        //act
        var (cheepsOnPage1, _) = await cheepRepo.GetCheepsAsync(1);
        var (cheepsOnPage2, _) = await cheepRepo.GetCheepsAsync(2);

        //assert
        Assert.Equal(32, cheepsOnPage1.Count);
        Assert.Equal(8, cheepsOnPage2.Count);


        //cleanup
        await context.Database.EnsureDeletedAsync();
        await context.DisposeAsync();
    }

    [Fact]
    public async Task GetCheepsFromAuthorAsync_AuthorWith40Cheeps_ReturnsCheepsFromAuthor()
    {
        //arrange
        ChirpDBContext context = await Util.CreateInMemoryDatabase();
        var achievementRepo = new AchievementRepository(context);
        var cheepRepo = new CheepRepository(context, achievementRepo);

        var a1 = new Author()
        {
            Name = "Roger Histand",
            Email = "Roger+Histand@hotmail.com",
            Cheeps = new List<Cheep>(),
            Followers = new List<AuthorFollower>(),
            Following = new List<AuthorFollower>()
        };

        context.Authors.Add(a1);

        for (int i = 0; i < 40; i++)
        {
            var cheep = new Cheep
            {
                Text = "" + i,
                AuthorId = a1.Id,
                TimeStamp = CurrentTime
            };

            context.Cheeps.AddRange(cheep);
        }
        await context.SaveChangesAsync();

        //act
        var (cheepsOnPage, _) = await cheepRepo.GetCheepsFromAuthorAsync(1, a1.Name);

        //assert
        foreach (var cheep in cheepsOnPage)
        {
            Assert.Equal(cheep.AuthorName, a1.Name);
        }

        //cleanup
        await context.Database.EnsureDeletedAsync();
        await context.DisposeAsync();
    }

    [Fact]
    public async Task FollowAuthorAsync_2Authors_ShouldAddAuthorFollowerRecord()
    {
        //arrange
        ChirpDBContext context = await Util.CreateInMemoryDatabase();
        IAchievementRepository achievementRepository = new AchievementRepository(context);
        IAuthorRepository authorRepository = new AuthorRepository(context, achievementRepository);
        
        Achievement ach3 = new Achievement { AchievementId = 3, Title = "Branching Out", Description = "You followed your first Chirper. Every great tree starts with a single branch.", ImagePath = "/images/Badges/First-following-badge.png" };
        Achievement ach4 = new Achievement { AchievementId = 4, Title = "Social Magnet", Description = "Someone followed you. You must be cheeping some good stuff.", ImagePath = "/images/Badges/First-follower-badge.png" };

        context.Achievements.AddRange(ach3, ach4);

        Author followed = new Author
        {
            Name = "TestUser1",
            Email = "test1@example.dk",
            Cheeps = new List<Cheep>(),
            Followers = new List<AuthorFollower>(),
            Following = new List<AuthorFollower>()
        };
        Author following = new Author
        {
            Name = "TestUser2",
            Email = "test2@example.dk",
            Cheeps = new List<Cheep>(),
            Followers = new List<AuthorFollower>(),
            Following = new List<AuthorFollower>()
        };
        context.Authors.AddRange(followed, following);
        await context.SaveChangesAsync();


        //act
        await authorRepository.FollowAuthorAsync(followed.Id, following.Id);
        var isFollowing = await context.AuthorFollowers
         .AnyAsync(af => af.FollowerId == followed.Id && af.FollowingId == following.Id);

        //assert
        Assert.True(isFollowing);


        //cleanup
        await context.Database.EnsureDeletedAsync();
        await context.DisposeAsync();
    }

    [Fact]
    public async Task UnfollowAuthorAsync_AuthorFollowingAuthor_ShouldRemoveAuthorFollowerRecord()
    {
        //arrange
        ChirpDBContext context = await Util.CreateInMemoryDatabase();
        IAchievementRepository achievementRepository = new AchievementRepository(context);
        IAuthorRepository authorRepository = new AuthorRepository(context, achievementRepository);

        Author followed = new Author
        {
            Name = "TestUser1",
            Email = "test1@example.dk",
            Cheeps = new List<Cheep>(),
            Followers = new List<AuthorFollower>(),
            Following = new List<AuthorFollower>()
        };
        Author following = new Author
        {
            Name = "TestUser2",
            Email = "test2@example.dk",
            Cheeps = new List<Cheep>(),
            Followers = new List<AuthorFollower>(),
            Following = new List<AuthorFollower>()
        };
        context.Authors.AddRange(followed, following);
     
        context.AuthorFollowers.Add(new AuthorFollower { FollowerId = followed.Id, FollowingId = following.Id });

        await context.SaveChangesAsync();

        //act
        await authorRepository.UnfollowAuthorAsync(followed.Id, following.Id);
        var isFollowing = await context.AuthorFollowers
               .AnyAsync(af => af.FollowerId == followed.Id && af.FollowingId == following.Id);
        
        
        //assert
        Assert.False(isFollowing);


        //cleanup
        await context.Database.EnsureDeletedAsync();
        await context.DisposeAsync();
    }

    [Fact]
    public async Task GetCheepsFromUserTimelineAsync_AuthorFollowingAuthorBothWithCheep_Returns2Cheeps()
    {
        //arrange
        ChirpDBContext context = await Util.CreateInMemoryDatabase();
        IAchievementRepository achievementRepository = new AchievementRepository(context);
        ICheepRepository cheepRepository = new CheepRepository(context, achievementRepository);

        Achievement ach1 = new Achievement() { Title = "Rookie Chirper", Description = "Welcome aboard! You signed up successfully to Chirp", ImagePath = "/images/Badges/Signup-badge.png" };
        Achievement ach2 = new Achievement() { Title = "Novice Cheepster", Description = "Congratulations! You created your first Cheep.", ImagePath = "/images/Badges/First-cheep-badge.png" };
        Achievement ach3 = new Achievement() { Title = "Branching Out", Description = "You followed your first Chirper. Every great tree starts with a single branch.", ImagePath = "/images/Badges/First-following-badge.png" };
        Achievement ach4 = new Achievement() { Title = "Social Magnet", Description = "Someone followed you. You must be cheeping some good stuff.", ImagePath = "/images/Badges/First-follower-badge.png" };

        context.Achievements.AddRange(ach1, ach2, ach3, ach4);

        Author followed = new Author
        {
            Name = "TestUser1",
            Email = "test1@example.dk",
            Cheeps = new List<Cheep>(),
            Followers = new List<AuthorFollower>(),
            Following = new List<AuthorFollower>()
        };
        Author following = new Author
        {
            Name = "TestUser2",
            Email = "test2@example.dk",
            Cheeps = new List<Cheep>(),
            Followers = new List<AuthorFollower>(),
            Following = new List<AuthorFollower>()
        };
        context.Authors.AddRange(followed, following);
        context.Cheeps.AddRange(
            new Cheep
            {
                Text = "Hello World!",
                AuthorId = followed.Id,
                TimeStamp = CurrentTime,
            },
            new Cheep
            {
                Text = "Another Cheep, hell yeah",
                AuthorId = following.Id,
                TimeStamp = CurrentTime,
            }
        );

        context.AuthorFollowers.Add(new AuthorFollower { FollowerId = followed.Id, FollowingId = following.Id });

        await context.SaveChangesAsync();

        
        //act
        var (timeline, _) = await cheepRepository.GetCheepsFromUserTimelineAsync(1, followed.Id);


        //assert
        Assert.Equal(2, timeline.Count);
        Assert.Contains(timeline, c => c.AuthorName == following.Name);
        Assert.Contains(timeline, c => c.AuthorName == followed.Name);


        //cleanup
        await context.Database.EnsureDeletedAsync();
        await context.DisposeAsync();
    }

    [Fact]
    public async Task GetFollowingAsync_OneAuthorFollowAnother_ReturnsFollowedAuthorName()
    {
        //arrange
        var context = await Util.CreateInMemoryDatabase();
        var achievementRepository = new AchievementRepository(context);
        var authorRepository = new AuthorRepository(context, achievementRepository);
        
        var author1 = new Author()
        {
            Name = "TestUser1",
            Email = "test1@example.dk",
            Cheeps = new List<Cheep>(),
            Followers = new List<AuthorFollower>(),
            Following = new List<AuthorFollower>()
        };
        
        var author2 = new Author()
        {
            Name = "TestUser2",
            Email = "test2@example.dk",
            Cheeps = new List<Cheep>(),
            Followers = new List<AuthorFollower>(),
            Following = new List<AuthorFollower>()
        };
        
        context.Authors.AddRange(author1, author2);
        
        // Make author 1 follow author 2
        context.AuthorFollowers.Add(new AuthorFollower { FollowerId = author1.Id, FollowingId = author2.Id });
        
        await context.SaveChangesAsync();
        
        //act
        var author1Following = await authorRepository.GetFollowingAsync(author1.Id);
        
        //assert
        Assert.Single(author1Following);
        Assert.Equal("TestUser2", author1Following.First());
        
        //cleanup
        await context.Database.EnsureDeletedAsync();
        await context.DisposeAsync();
    }
    
    [Fact]
    public async Task GetFollowersAsync_OneAuthorFollowAnother_ReturnsFollowerName()
    {
        //arrange
        var context = await Util.CreateInMemoryDatabase();
        var achievementRepository = new AchievementRepository(context);
        var authorRepository = new AuthorRepository(context, achievementRepository);
        
        var author1 = new Author()
        {
            Name = "TestUser1",
            Email = "test1@example.dk",
            Cheeps = new List<Cheep>(),
            Followers = new List<AuthorFollower>(),
            Following = new List<AuthorFollower>()
        };
        
        var author2 = new Author()
        {
            Name = "TestUser2",
            Email = "test2@example.dk",
            Cheeps = new List<Cheep>(),
            Followers = new List<AuthorFollower>(),
            Following = new List<AuthorFollower>()
        };
        
        context.Authors.AddRange(author1, author2);
        
        // Make author 1 follow author 2
        context.AuthorFollowers.Add(new AuthorFollower { FollowerId = author1.Id, FollowingId = author2.Id });
        
        await context.SaveChangesAsync();
        
        //act
        var author2Followers = await authorRepository.GetFollowersAsync(author2.Id);
        
        //assert
        Assert.Single(author2Followers);
        Assert.Equal("TestUser1", author2Followers.First());
        
        //cleanup
        await context.Database.EnsureDeletedAsync();
        await context.DisposeAsync();
    }

    [Theory]
    [InlineData("My new Bio", "My new Bio")]
    [InlineData(null, null)]
    public async Task UpdateBioAsync_TextAndNullBio_ShouldUpdateAuthorBio(string? bioText, string? expected)
    {
        //arrange
        var context = await Util.CreateInMemoryDatabase();
        var achievementRepository = new AchievementRepository(context);
        var authorRepository = new AuthorRepository(context, achievementRepository);
        
        var author = new Author()
        {
            Name = "TestUser1",
            Email = "test1@example.dk",
            Cheeps = new List<Cheep>(),
            Followers = new List<AuthorFollower>(),
            Following = new List<AuthorFollower>()
        };

        context.Authors.Add(author);

        await context.SaveChangesAsync();
        
        //act
        await authorRepository.UpdateBioAsync(author.Id, bioText);
        
        //arrange
        Assert.Equal(expected, author.Bio);
        
        //cleanup
        await context.Database.EnsureDeletedAsync();
        await context.DisposeAsync();
    }
    
    
    [Fact]
    public async Task AddNewAuthorAchievementAsync_SingleAchievementAndAuthor_AddsAuthorAchievementRecord()
    {
        //arrange
        var context = await Util.CreateInMemoryDatabase();
        var achievementRepository = new AchievementRepository(context);
        
        var achievement = new Achievement() { AchievementId = 1, Title = "Rookie Chirper", Description = "Welcome aboard! You signed up successfully to Chirp", ImagePath = "/images/Badges/Signup-badge.png" };
        context.Achievements.Add(achievement);
        
        var author = new Author()
        {
            Name = "TestUser1",
            Email = "test1@example.dk",
            Cheeps = new List<Cheep>(),
            Followers = new List<AuthorFollower>(),
            Following = new List<AuthorFollower>()
        };
        
        context.Authors.Add(author);
        
        await context.SaveChangesAsync();
        
        //act
        await achievementRepository.AddNewAuthorAchievementAsync(author.Id, achievement.AchievementId);
        var authorAchievement = context.AuthorAchievements.First();

        //assert
        Assert.NotNull(authorAchievement);
        Assert.Equal(authorAchievement.AuthorId, author.Id);
        Assert.Equal(authorAchievement.AchievementId, achievement.AchievementId);
    }
    
    [Fact]
    public async Task GetAuthorNewestAchievementAsync_AuthorWith2Achievements_ReturnsNewestAuthorAchievement()
    {
        //arrange
        var context = await Util.CreateInMemoryDatabase();
        var achievementRepository = new AchievementRepository(context);
        
        var achievement1 = new Achievement() { AchievementId = 1, Title = "Rookie Chirper", Description = "Welcome aboard! You signed up successfully to Chirp", ImagePath = "/images/Badges/Signup-badge.png" };
        var achievement2 = new Achievement() { AchievementId = 2, Title = "Novice Cheepster", Description = "Congratulations! You created your first Cheep.", ImagePath = "/images/Badges/First-cheep-badge.png" };
        context.Achievements.AddRange(achievement1, achievement2);
        
        var author = new Author()
        {
            Name = "TestUser1",
            Email = "test1@example.dk",
            Cheeps = new List<Cheep>(),
            Followers = new List<AuthorFollower>(),
            Following = new List<AuthorFollower>()
        };
        context.Authors.Add(author);
        
        var authorAchievement1 = new AuthorAchievement() { AuthorId = author.Id, AchievementId = achievement1.AchievementId, AchievedAt = DateTime.Parse("2024-05-01 15:00:00")};
        var authorAchievement2 = new AuthorAchievement() { AuthorId = author.Id, AchievementId = achievement2.AchievementId, AchievedAt = DateTime.Parse("2024-05-02 15:00:00") };
        context.AuthorAchievements.AddRange(authorAchievement1, authorAchievement2);
        
        await context.SaveChangesAsync();
        
        //act
        var achievement = await achievementRepository.GetAuthorNewestAchievementAsync(author.Id);

        //assert
        Assert.NotNull(achievement);
        Assert.Equal(2, achievement.AchievementId);
    }
    
    [Fact]
    public async Task GetAuthorAchievementsAsync_AuthorWith2Achievements_ReturnsListOfAchievements()
    {
        //arrange
        var context = await Util.CreateInMemoryDatabase();
        var achievementRepository = new AchievementRepository(context);
        
        var achievement1 = new Achievement() { AchievementId = 1, Title = "Rookie Chirper", Description = "Welcome aboard! You signed up successfully to Chirp", ImagePath = "/images/Badges/Signup-badge.png" };
        var achievement2 = new Achievement() { AchievementId = 2, Title = "Novice Cheepster", Description = "Congratulations! You created your first Cheep.", ImagePath = "/images/Badges/First-cheep-badge.png" };
        context.Achievements.AddRange(achievement1, achievement2);
        
        var author = new Author()
        {
            Name = "TestUser1",
            Email = "test1@example.dk",
            Cheeps = new List<Cheep>(),
            Followers = new List<AuthorFollower>(),
            Following = new List<AuthorFollower>()
        };
        context.Authors.Add(author);
        
        var authorAchievement1 = new AuthorAchievement() { AuthorId = author.Id, AchievementId = achievement1.AchievementId, AchievedAt = DateTime.Parse("2024-05-01 15:00:00")};
        var authorAchievement2 = new AuthorAchievement() { AuthorId = author.Id, AchievementId = achievement2.AchievementId, AchievedAt = DateTime.Parse("2024-05-02 15:00:00") };
        context.AuthorAchievements.AddRange(authorAchievement1, authorAchievement2);
        
        await context.SaveChangesAsync();
        
        //act
        var achievements = await achievementRepository.GetAuthorAchievementsAsync(author.Id);

        //assert
        Assert.Equal(2, achievements.Count);
    }
}