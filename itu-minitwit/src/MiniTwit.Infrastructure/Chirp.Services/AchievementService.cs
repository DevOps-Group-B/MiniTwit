using Chirp.Core.Models;
using Chirp.Core.Repositories;
using Chirp.Core.Services;

namespace Chirp.Infrastructure.Chirp.Services;

/// <summary>
/// AchievementService is for buissness logic regarding achievements.
/// </summary>
public class AchievementService : IAchievementService
{
    private readonly IAchievementRepository _achievementRepository;

    public AchievementService(IAchievementRepository achievementRepository)
    {
        _achievementRepository = achievementRepository;
    }
    
    public async Task AddNewAuthorAchievementAsync(string authorId, int achievementId)
    {
        await _achievementRepository.AddNewAuthorAchievementAsync(authorId, achievementId);
    }
    
    public async Task<Achievement?> GetAuthorNewestAchievementAsync(string authorId)
    {
        return await _achievementRepository.GetAuthorNewestAchievementAsync(authorId);
    }

    public async Task<List<Achievement>> GetAuthorAchievementsAsync(string authorId)
    {
        return await _achievementRepository.GetAuthorAchievementsAsync(authorId);
    }
}