using Chirp.Core.DTOs;
using Chirp.Core.Models;

namespace Chirp.Core.Services;

/// <summary>
/// ICheepService is for defining the methods for the CheepService
/// </summary>
public interface ICheepService
{
    public Task<(List<CheepDTO> cheeps, int totalCheepCount)> GetCheepsAsync(int page);
    public Task<(List<CheepDTO> cheeps, int totalCheepCount)> GetCheepsAmountAsync(int numberOfCheeps);
    public Task<(List<CheepDTO> cheeps, int totalCheepCount)> GetCheepsFromAuthorAsync(int page, string authorId);
    public Task<(List<CheepDTO> cheeps, int totalCheepCount)> GetCheepsFromAuthorAmountAsync(int numberOfCheeps, string authorId);
    public Task<(List<CheepDTO> cheeps, int totalCheepCount)> GetCheepsFromUserTimelineAsync(int page, string authorId);
    public Task PostCheepAsync(Cheep cheep);
}