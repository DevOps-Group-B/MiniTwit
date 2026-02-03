using Chirp.Core.Models;
using Chirp.Core.Services;
using Chirp.Web.Pages.Base;
using Chirp.Web.Pages.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chirp.Web.Pages;
/// <summary>
/// UserTimelineModel class is for displaying the user's timeline.
/// Here the user can see their own cheeps and the cheeps of the authors they follow.
/// Users can also update their bio and see their achievements.
/// </summary>
public class UserTimelineModel : BaseCheepTimelinePage
{
    [BindProperty]
    public BioTextModel BioText { get; set; }

    public UserTimelineModel(ICheepService service, IAuthorService authorService, UserManager<Author> userManager)
        : base(service, authorService, userManager)
    {
        BioText = new BioTextModel();
    }

    public async Task<ActionResult> OnGet(string author)
    {
        var pageQuery = Request.Query["page"];
        PageNumber = int.TryParse(pageQuery, out var page) ? Math.Max(page, 1) : 1;
        AuthenticatedAuthor = await GetAuthenticatedAuthor();

        var authorRequest = await _authorService.GetAuthorByNameAsync(author);
        if (authorRequest == null) return Page();
        
        BioText.Bio = authorRequest.Bio;

        if(User.Identity!.IsAuthenticated && AuthenticatedAuthor != null){
            if(AuthenticatedAuthor.Name == author){
                (Cheeps, CheepCount) = await _cheepService.GetCheepsFromUserTimelineAsync(PageNumber, authorRequest.Id);
            } else {
                (Cheeps, CheepCount) = await _cheepService.GetCheepsFromAuthorAsync(PageNumber, authorRequest.Id);
            }
            await PopulateFollows();
        } else {
            (Cheeps, CheepCount) = await _cheepService.GetCheepsFromAuthorAsync(PageNumber, authorRequest.Id);
        }

        return Page();
    }
    
    public async Task<ActionResult> OnPostUpdateBio(string? newBio)
    {
        if (User.Identity?.IsAuthenticated != true) return Page();
        AuthenticatedAuthor = await GetAuthenticatedAuthor();
        if(AuthenticatedAuthor == null) return Page();
        
        newBio = newBio?.Trim();
        
        if (newBio?.Length > 300)
        {
            return Page();
        }

        await _authorService.UpdateBioAsync(AuthenticatedAuthor.Id, newBio);
        
        return RedirectToPage();
    }
}