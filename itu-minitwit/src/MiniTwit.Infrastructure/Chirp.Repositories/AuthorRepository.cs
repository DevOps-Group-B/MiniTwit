using Chirp.Core.DTOs;
using Chirp.Core.Models;
using Chirp.Core.Repositories;
using Chirp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.Chirp.Repositories;

/// <summary>
/// AuthorRepository is for interactions with the database regarding authors.
/// </summary>
public class AuthorRepository : IAuthorRepository
{
    private readonly ChirpDBContext _dbContext;
    private readonly IAchievementRepository _achievementRepository;

    public AuthorRepository(ChirpDBContext dbContext, IAchievementRepository achievementRepository)
    {
        _dbContext = dbContext;
        _achievementRepository = achievementRepository;
    }
    
    public async Task<AuthorDTO?> GetAuthorByNameAsync(string name)
    {
        var query = from a in _dbContext.Authors
            where a.Name == name
            select new AuthorDTO { Id = a.Id, Name = a.Name, Email = a.Email, Bio = a.Bio };

        return await query.FirstOrDefaultAsync();
    }

    public async Task<AuthorDTO?> GetAuthorByEmailAsync(string email)
    {
        var query = from a in _dbContext.Authors
                    where a.Email == email
                    select new AuthorDTO { Id = a.Id, Name = a.Name, Email = a.Email, Bio = a.Bio };

        return await query.FirstOrDefaultAsync();
    }

    public async Task<AuthorDTO> NewAuthorAsync(string authorName, string authorEmail)
    {
        var newAuthor = new Author()
        {
            Name = authorName,
            Email = authorEmail,
            Cheeps = new List<Cheep>(),
            Followers = new List<AuthorFollower>(),
            Following = new List<AuthorFollower>()
        };

        _dbContext.Authors.Add(newAuthor);
        await _dbContext.SaveChangesAsync();

        return new AuthorDTO { Id = newAuthor.Id, Name = newAuthor.Name, Email = newAuthor.Email, Bio = newAuthor.Bio };
    }

    public async Task FollowAuthorAsync(string currentAuthorId, string targetAuthorId)
    {
        var hasAchievementFollow = await _dbContext.AuthorAchievements.AnyAsync(a => a.AuthorId == currentAuthorId && a.AchievementId == 3);
        var hasAchievementFollowed = await _dbContext.AuthorAchievements.AnyAsync(a => a.AuthorId == targetAuthorId && a.AchievementId == 4);
        
        var followRelation = new AuthorFollower
        {
            FollowerId = currentAuthorId,
            FollowingId = targetAuthorId
        };

        _dbContext.AuthorFollowers.Add(followRelation);
        await _dbContext.SaveChangesAsync();

        if (!hasAchievementFollow) await _achievementRepository.AddNewAuthorAchievementAsync(currentAuthorId, 3);
        if (!hasAchievementFollowed) await _achievementRepository.AddNewAuthorAchievementAsync(targetAuthorId, 4);
    }

    public async Task UnfollowAuthorAsync(string currentAuthorId, string targetAuthorId)
    {
        var followRelation = await _dbContext.AuthorFollowers.SingleOrDefaultAsync(a =>
            a.Follower.Id == currentAuthorId && a.Following.Id == targetAuthorId);

        _dbContext.AuthorFollowers.Remove(followRelation!);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> IsFollowingAsync(string currentAuthorId, string targetAuthorId)
    {
        var followRelation = await _dbContext.AuthorFollowers.SingleOrDefaultAsync(a =>
            a.Follower.Id == currentAuthorId && a.Following.Id == targetAuthorId);

        return followRelation != null;
    }

    public async Task<List<string>> GetFollowingAsync(string authorId)
    {
        var query = (
            from a in _dbContext.AuthorFollowers
            where a.FollowerId == authorId
            select a.Following.Name);
        
        return await query.ToListAsync();
    }
    
    public async Task<List<string>> GetFollowersAsync(string authorId)
    {
        var query = (
            from a in _dbContext.AuthorFollowers
            where a.FollowingId == authorId
            select a.Follower.Name);
        
        return await query.ToListAsync();
    }
    
    public async Task<string?> UpdateBioAsync(string authorId, string? newBio)
    {
        var authorFromDb = await _dbContext.Authors.FindAsync(authorId);
        if (authorFromDb == null) return null;
        
        authorFromDb.Bio = newBio;
        
        _dbContext.Authors.Update(authorFromDb);
        
        await _dbContext.SaveChangesAsync();

        return newBio;
    }
}