using Chirp.Core.DTOs;
using Chirp.Core.Models;

namespace Chirp.Core.Repositories;

/// <summary>
/// ICheepRepository is for defining the methods for the CheepRepository
/// </summary>
public interface ICheepRepository
{
    public Task<(List<CheepDTO> cheeps, int totalCheepCount)> GetCheepsAsync(int page);
    public Task<(List<CheepDTO> cheeps, int totalCheepCount)> GetCheepsAmountAsync(int numberOfCheeps);
    public Task<(List<CheepDTO> cheeps, int totalCheepCount)> GetCheepsFromAuthorAsync(int page, string authorId);
    public Task<(List<CheepDTO> cheeps, int totalCheepCount)> GetCheepsFromAuthorAmountAsync(int numberOfCheeps, string authorId);
    public Task<(List<CheepDTO> cheeps, int totalCheepCount)> GetCheepsFromUserTimelineAsync(int page, string authorId);
    public Task PostCheepAsync(Cheep cheep);
}