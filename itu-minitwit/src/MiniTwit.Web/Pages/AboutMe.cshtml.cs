using Chirp.Core.Models;
using Chirp.Core.Services;
using Chirp.Web.Pages.Base;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chirp.Web.Pages;
/// <summary>
/// AboutMeModel class is for displaying the current author's information.
/// This includes the author's name, email, cheeps, and achievements.
/// The forgetMe button allows the user to forget their account.
/// This page helps us comply with GDPR regulations. 
/// </summary>
public class AboutMeModel : BaseCheepDisplayPage
{
    protected readonly IAchievementService _achievementService;
    public required List<Achievement> Achievements { get; set; }
    

    public AboutMeModel(ICheepService cheepService, IAuthorService authorService, IAchievementService achievementService, UserManager<Author> userManager) : base(cheepService, authorService, userManager)
    {
        _achievementService = achievementService;
        Achievements = new List<Achievement>();
    }

    public async Task<ActionResult> OnGet()
    {
        var pageQuery = Request.Query["page"];
        PageNumber = int.TryParse(pageQuery, out var page) ? Math.Max(page, 1) : 1;
        AuthenticatedAuthor = await GetAuthenticatedAuthor();
        
        if(User.Identity!.IsAuthenticated && AuthenticatedAuthor != null){
            (Cheeps, CheepCount) = await _cheepService.GetCheepsFromAuthorAsync(PageNumber, AuthenticatedAuthor.Id);
            Achievements = await _achievementService.GetAuthorAchievementsAsync(AuthenticatedAuthor.Id);
        }
        return Page();
    }


    public async Task<ActionResult> OnPostForgetMe(){
        if(!User.Identity!.IsAuthenticated) return Page();

        var currentAuthor = await _userManager.GetUserAsync(User);

        await _userManager.DeleteAsync(currentAuthor!);

        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        return RedirectToPage("/Public");
    }
}