using Chirp.Core.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTest;
using System;
using Xunit;
using Chirp.Infrastructure.Data;
using Util;

/// <summary>
/// IntegrationTest class is designed to test the interactions between different components in the Chirp application.
/// It tests the public timeline, private timeline, bio, cheeps, and default messages.
/// </summary>
public class IntegrationTest : IClassFixture<IntegrationTestFixture<Program>>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestFixture<Program> _factory;

    public IntegrationTest(IntegrationTestFixture<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions() { AllowAutoRedirect = true, HandleCookies = true });
    }
    
    [Fact]
    public async void CanSeePublicTimeline()
    {
        var response = await _client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
    
        Assert.Contains("MiniTwit!", content);
        Assert.Contains("Public Timeline", content);   
    }
    
    [Theory]
    [InlineData("Helge")]
    [InlineData("Adrian")]
    public async void CanSeePrivateTimeline(string authorName)
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var serviceProvider = scope.ServiceProvider;
            var dbContext = serviceProvider.GetRequiredService<ChirpDBContext>();
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();

            var author1 = new Author() { Name = "Helge", UserName = "ropf@itu.dk", Email = "ropf@itu.dk", EmailConfirmed = true, Cheeps = new List<Cheep>(), Followers = new List<AuthorFollower>(), Following = new List<AuthorFollower>() };
            var author2 = new Author() { Name = "Adrian", UserName = "adho@itu.dk", Email = "adho@itu.dk", EmailConfirmed = true, Cheeps = new List<Cheep>(), Followers = new List<AuthorFollower>(), Following = new List<AuthorFollower>() };
            
            dbContext.Authors.AddRange(author1, author2);
            await dbContext.SaveChangesAsync();
        }
        
        var response = await _client.GetAsync($"/{authorName}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
    
        Assert.Contains("MiniTwit!", content);
        Assert.Contains($"{authorName}'s Timeline", content);
    }

    [Theory]
    [InlineData("Helge", "Pro golfer")]
    [InlineData("Adrian", "Je suis...")]
    public async void CanSeeBio(string authorName, string bio)
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var serviceProvider = scope.ServiceProvider;
            var dbContext = serviceProvider.GetRequiredService<ChirpDBContext>();
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();

            var author1 = new Author() { Name = "Helge", UserName = "ropf@itu.dk", Email = "ropf@itu.dk", EmailConfirmed = true, Cheeps = new List<Cheep>(), Followers = new List<AuthorFollower>(), Following = new List<AuthorFollower>(), Bio = "Pro golfer" };
            var author2 = new Author() { Name = "Adrian", UserName = "adho@itu.dk", Email = "adho@itu.dk", EmailConfirmed = true, Cheeps = new List<Cheep>(), Followers = new List<AuthorFollower>(), Following = new List<AuthorFollower>(), Bio = "Je suis..." };
            
            dbContext.Authors.AddRange(author1, author2);
            await dbContext.SaveChangesAsync();
        }
        
        var response = await _client.GetAsync($"/{authorName}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
    
        Assert.Contains(bio, content);
    }
    
    [Fact]
    public async void CanSeeAuthorCheeps()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var serviceProvider = scope.ServiceProvider;
            var dbContext = serviceProvider.GetRequiredService<ChirpDBContext>();
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();

            var author = new Author() { Name = "Helge", UserName = "ropf@itu.dk", Email = "ropf@itu.dk", EmailConfirmed = true, Cheeps = new List<Cheep>(), Followers = new List<AuthorFollower>(), Following = new List<AuthorFollower>() };
            
            var cheep1 = new Cheep
            {
                Text = "Hello World!",
                AuthorId = author.Id,
                TimeStamp = DateTime.Now
            };
            
            var cheep2 = new Cheep
            {
                Text = "My precious Cheep",
                AuthorId = author.Id,
                TimeStamp = DateTime.Now
            };
            
            dbContext.Authors.Add(author);
            dbContext.Cheeps.AddRange(cheep1, cheep2);
            
            await dbContext.SaveChangesAsync();
        }
        
        var response = await _client.GetAsync($"/Helge");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
    
        Assert.Contains("Hello World!", content);
        Assert.Contains("My precious Cheep", content);
    }
    
    [Fact]
    public async void CanSeeAllCheeps()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var serviceProvider = scope.ServiceProvider;
            var dbContext = serviceProvider.GetRequiredService<ChirpDBContext>();
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();

            var author1 = new Author() { Name = "Helge", UserName = "ropf@itu.dk", Email = "ropf@itu.dk", EmailConfirmed = true, Cheeps = new List<Cheep>(), Followers = new List<AuthorFollower>(), Following = new List<AuthorFollower>() };
            var author2 = new Author() { Name = "Adrian", UserName = "adho@itu.dk", Email = "adho@itu.dk", EmailConfirmed = true, Cheeps = new List<Cheep>(), Followers = new List<AuthorFollower>(), Following = new List<AuthorFollower>() };
            var author3 = new Author() { Name = "Hans", UserName = "hans@itu.dk", Email = "hans@itu.dk", EmailConfirmed = true, Cheeps = new List<Cheep>(), Followers = new List<AuthorFollower>(), Following = new List<AuthorFollower>() };
            
            var cheep1 = new Cheep
            {
                Text = "Hello World!",
                AuthorId = author1.Id,
                TimeStamp = DateTime.Now
            };
            
            var cheep2 = new Cheep
            {
                Text = "My precious Cheep",
                AuthorId = author2.Id,
                TimeStamp = DateTime.Now
            };
            
            var cheep3 = new Cheep
            {
                Text = "Testing cheeps",
                AuthorId = author3.Id,
                TimeStamp = DateTime.Now
            };
            
            dbContext.Authors.AddRange(author1, author2, author3);
            dbContext.Cheeps.AddRange(cheep1, cheep2, cheep3);
            
            await dbContext.SaveChangesAsync();
        }
        
        var response = await _client.GetAsync($"/");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
    
        Assert.Contains("Hello World!", content);
        Assert.Contains("My precious Cheep", content);
        Assert.Contains("Testing cheeps", content);
    }
    
    [Fact]
    public async void DefaultMessageOnUserTimelineWhenNoCheeps()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var serviceProvider = scope.ServiceProvider;
            var dbContext = serviceProvider.GetRequiredService<ChirpDBContext>();
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();

            var author = new Author() { Name = "Helge", UserName = "ropf@itu.dk", Email = "ropf@itu.dk", EmailConfirmed = true, Cheeps = new List<Cheep>(), Followers = new List<AuthorFollower>(), Following = new List<AuthorFollower>() };
            
            dbContext.Authors.Add(author);
            
            await dbContext.SaveChangesAsync();
        }
        
        var response = await _client.GetAsync($"/Helge");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
    
        Assert.Contains("There are no cheeps so far.", content);
    }
    
    [Fact]
    public async void DefaultMessageOnPublicTimelineWhenNoCheeps()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var serviceProvider = scope.ServiceProvider;
            var dbContext = serviceProvider.GetRequiredService<ChirpDBContext>();
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
        }
        
        var response = await _client.GetAsync($"/");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
    
        Assert.Contains("There are no cheeps so far.", content);
    }
}