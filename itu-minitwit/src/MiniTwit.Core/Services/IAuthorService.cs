using Chirp.Core.DTOs;

namespace Chirp.Core.Services;

/// <summary>
/// IAuthorService interface is for defining the methods for the AuthorService
/// </summary>
public interface IAuthorService
{
    public Task<AuthorDTO?> GetAuthorByNameAsync(string name);
    public Task<AuthorDTO?> GetAuthorByEmailAsync(string email);
    public Task FollowAuthorAsync(string currentAuthorName, string targetAuthorName);
    public Task UnfollowAuthorAsync(string currentAuthorName, string targetAuthorName);
    public Task<bool> IsFollowingAsync(string currentAuthorId, string targetAuthorId);
    public Task<List<string>> GetFollowingAsync(string authorId);
    public Task<List<string>> GetFollowersAsync(string authorId);
    public Task<string?> UpdateBioAsync(string authorId, string? newBio);
}