using Chirp.Core.DTOs;
using Chirp.Core.Models;
using Chirp.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages.Base;
/// <summary>
/// BaseCheepDisplayPage is for all pages that display cheeps.
/// In our case, this is the Public timeline, user timeline, and aboutMe page.
/// </summary>
public class BaseCheepDisplayPage : PageModel
{
    public required List<CheepDTO> Cheeps { get; set; }
    public required int PageNumber { get; set; }
    public required int CheepCount { get; set; }
    public required AuthorDTO? AuthenticatedAuthor { get; set; }
    public Dictionary<string, bool> Follows { get; set; }
    protected readonly IAuthorService _authorService;
    protected readonly ICheepService _cheepService;
    protected readonly UserManager<Author> _userManager;

    public BaseCheepDisplayPage(ICheepService cheepService, IAuthorService authorService, UserManager<Author> userManager)
    {
        _authorService = authorService;
        _cheepService = cheepService;
        _userManager = userManager;
        Cheeps = new List<CheepDTO>();
        PageNumber = 1;
        Follows = new Dictionary<string, bool>();
    }

    public async Task<AuthorDTO?> GetAuthenticatedAuthor()
    {
        if (!User.Identity!.IsAuthenticated) return null;
        
        var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
        if (email == null) return null;
        
        var author = await _authorService.GetAuthorByEmailAsync(email);
        return author;
    }
}