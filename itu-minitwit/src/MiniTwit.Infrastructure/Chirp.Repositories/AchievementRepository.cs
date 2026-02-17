using Chirp.Core.Models;
using Chirp.Core.Repositories;
using Chirp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.Chirp.Repositories;

public class AchievementRepository : IAchievementRepository
{
    private readonly ChirpDBContext _dbContext;
    
    public AchievementRepository(ChirpDBContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task AddNewAuthorAchievementAsync(string authorId, int achievementId)
    {
        var authorAchievement = new AuthorAchievement()
        {
            AuthorId = authorId,
            AchievementId = achievementId
        };

        _dbContext.AuthorAchievements.Add(authorAchievement);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Achievement?> GetAuthorNewestAchievementAsync(string authorId)
    {
        var query = (
            from ach in _dbContext.AuthorAchievements
            where ach.AuthorId == authorId
            orderby ach.AchievedAt descending
            select ach.Achievement);

        return await query.FirstOrDefaultAsync();
    }
    
    public async Task<List<Achievement>> GetAuthorAchievementsAsync(string authorId)
    {
        var query = (
            from ach in _dbContext.AuthorAchievements
            where ach.AuthorId == authorId
            orderby ach.AchievedAt descending
            select ach.Achievement);

        return await query.ToListAsync();
    }
}