using Chirp.Core.Models;
using Chirp.Core.Services;
using Chirp.Web.Pages.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chirp.Web.Pages.Base;
/// <summary>
/// BaseCheepTimelinePage is for all timeline pages that display cheeps.
/// In our case, this is the Public timeline and user timeline.
/// </summary>
public class BaseCheepTimelinePage : BaseCheepDisplayPage
{
    [BindProperty] 
    public CheepFormTextModel FormData { get; set; } = null!;


    public BaseCheepTimelinePage(ICheepService cheepService, IAuthorService authorService,
        UserManager<Author> userManager) : base(cheepService, authorService, userManager)
    {}

    public async Task<ActionResult> OnPost()
    {
        if (User.Identity?.IsAuthenticated != true) return Page();
        if (FormData.Message == null || FormData.Message.Length > 300) return Page();
        AuthenticatedAuthor = await GetAuthenticatedAuthor();
        if(AuthenticatedAuthor == null) return Page();
        
        await _cheepService.PostCheepAsync(new Cheep
        {
            AuthorId = AuthenticatedAuthor.Id,
            Text = FormData.Message,
            TimeStamp = DateTime.Now
        });

        return RedirectToPage();
    }

    public async Task<ActionResult> OnPostFollowAuthor(string targetAuthorId)
    {
        if (User.Identity?.IsAuthenticated != true) return Page();
        AuthenticatedAuthor = await GetAuthenticatedAuthor();
        if(AuthenticatedAuthor == null) return Page();

        if (targetAuthorId == AuthenticatedAuthor.Id) return Page();

        await _authorService.FollowAuthorAsync(AuthenticatedAuthor.Id, targetAuthorId);

        return RedirectToPage();
    }
    
    public async Task<ActionResult> OnPostUnfollowAuthor(string targetAuthorId)
    {
        if (User.Identity?.IsAuthenticated != true) return Page();
        AuthenticatedAuthor = await GetAuthenticatedAuthor();
        if(AuthenticatedAuthor == null) return Page();
        
        if (targetAuthorId == AuthenticatedAuthor.Id) return Page();

        await _authorService.UnfollowAuthorAsync(AuthenticatedAuthor.Id, targetAuthorId);

        return RedirectToPage();
    }

    public async Task PopulateFollows(){
        foreach (var cheep in Cheeps)
        {
            var targetAuthorId = cheep.AuthorId;
            if(Follows.ContainsKey(targetAuthorId)){
                continue;
            }
            var isFollowing = await _authorService.IsFollowingAsync(AuthenticatedAuthor!.Id, targetAuthorId);
            Follows[targetAuthorId] = isFollowing;
        }
    }
}