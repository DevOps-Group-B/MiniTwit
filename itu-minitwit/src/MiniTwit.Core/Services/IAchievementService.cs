using Chirp.Core.Models;

namespace Chirp.Core.Services;

/// <summary>
/// IAchievementService interface is for defining the methods for the AchievementService
/// </summary>
public interface IAchievementService
{
    public Task AddNewAuthorAchievementAsync(string authorId, int achievementId);
    public Task<Achievement?> GetAuthorNewestAchievementAsync(string authorId);
    public Task<List<Achievement>> GetAuthorAchievementsAsync(string authorId);
}