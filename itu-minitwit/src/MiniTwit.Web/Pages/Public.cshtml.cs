using Chirp.Core.Models;
using Chirp.Core.Services;
using Chirp.Web.Pages.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chirp.Web.Pages;
/// <summary>
/// PublicModel class is for displaying cheeps on the public timeline.
/// Maximum of 32 cheeps per page.
/// </summary>
public class PublicModel : BaseCheepTimelinePage
{
    public PublicModel(ICheepService cheepService, IAuthorService authorService, UserManager<Author> userManager) 
        : base(cheepService, authorService, userManager) {}

    public async Task<ActionResult> OnGet()
    {
        var pageQuery = Request.Query["page"];
        PageNumber = int.TryParse(pageQuery, out var page) ? Math.Max(page, 1) : 1;
        (Cheeps, CheepCount) = await _cheepService.GetCheepsAsync(PageNumber);
        AuthenticatedAuthor = await GetAuthenticatedAuthor();

        if(User.Identity!.IsAuthenticated && AuthenticatedAuthor != null){
            await PopulateFollows();
        }
        
        return Page();
    }
}