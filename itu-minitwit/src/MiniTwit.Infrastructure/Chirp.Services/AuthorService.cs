using Chirp.Core.DTOs;
using Chirp.Core.Repositories;
using Chirp.Core.Services;

namespace Chirp.Infrastructure.Chirp.Services;

/// <summary>
/// AuthorService is for buissness logic regarding authors.
/// </summary>
public class AuthorService : IAuthorService
{
    private readonly IAuthorRepository _authorRepository;
    
    public AuthorService(IAuthorRepository authorRepository)
    {
        _authorRepository = authorRepository;
    }
    
    public async Task<AuthorDTO?> GetAuthorByNameAsync(string name)
    {
        return await _authorRepository.GetAuthorByNameAsync(name);
    }
    
    public async Task<AuthorDTO?> GetAuthorByEmailAsync(string email)
    {
        return await _authorRepository.GetAuthorByEmailAsync(email);
    }
    
    public async Task FollowAuthorAsync(string currentAuthorId, string targetAuthorId)
    {
        await _authorRepository.FollowAuthorAsync(currentAuthorId, targetAuthorId);
    }
    
    public async Task UnfollowAuthorAsync(string currentAuthorId, string targetAuthorId)
    {
        await _authorRepository.UnfollowAuthorAsync(currentAuthorId, targetAuthorId);
    }

    public async Task<bool> IsFollowingAsync(string currentAuthorId, string targetAuthorId)
    {
        return await _authorRepository.IsFollowingAsync(currentAuthorId, targetAuthorId);
    }
    
    public async Task<List<string>> GetFollowingAsync(string authorId){
        return await _authorRepository.GetFollowingAsync(authorId);
    }
    
    public async Task<List<string>> GetFollowingAmountAsync(int numberOfFollowing, string authorId){
        return await _authorRepository.GetFollowingAmountAsync(numberOfFollowing, authorId);
    }
    
    public async Task<List<string>> GetFollowersAsync(string authorId){
        return await _authorRepository.GetFollowersAsync(authorId);
    }
    
    public async Task<string?> UpdateBioAsync(string authorId, string? newBio)
    {
        return await _authorRepository.UpdateBioAsync(authorId, newBio);
    }
}