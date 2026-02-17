using Chirp.Core.DTOs;

namespace Chirp.Core.Repositories;

/// <summary>
/// IAuthorRepository interface is for defining the methods for the AuthorRepository
/// </summary>
public interface IAuthorRepository
{
    public Task<AuthorDTO?> GetAuthorByNameAsync(string name);
    public Task<AuthorDTO?> GetAuthorByEmailAsync(string email);
    public Task<AuthorDTO> NewAuthorAsync(string authorName, string authorEmail);
    public Task FollowAuthorAsync(string currentAuthorId, string targetAuthorId);
    public Task UnfollowAuthorAsync(string currentAuthorId, string targetAuthorId);
    public Task<bool> IsFollowingAsync(string currentAuthorId, string targetAuthorId);
    public Task<List<string>> GetFollowingAsync(string authorId);
    public Task<List<string>> GetFollowingAmountAsync(int numberOfFollowing, string authorId);
    public Task<List<string>> GetFollowersAsync(string authorId);
    public Task<string?> UpdateBioAsync(string authorId, string? newBio);
}