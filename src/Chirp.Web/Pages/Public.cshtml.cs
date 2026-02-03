using Chirp.Domain;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.VisualBasic;
using System;

namespace Chirp.Razor.Pages;

/// <summary>
/// Represents the public timeline page of Chirp, displaying all cheeps and allowing
/// authenticated users to post new messages, follow/unfollow other users, and like/unlike cheeps.
/// </summary>
public class PublicModel : PageModel
{
    private readonly ICheepService _service;

    [BindProperty(SupportsGet = true, Name = "pagenumber")]
    public int PageNumber { get; set; } = 1;
    public List<CheepDTO> Cheeps { get; set; } = new();
    public string? UserName { get; private set; }
    public User? CurrentUser { get; set; }
    public List<string> followedUsers { get; set; } = new();
    public PublicModel(ICheepService service)
    {
        _service = service;
    }

    /// <summary>
    /// Handles GET requests to the public timeline page.
    /// Retrieves the current user's data (if authenticated) and loads paginated cheeps.
    /// </summary>
    /// <returns>The <see cref="IActionResult"/> representing the rendered page.</returns>
    public async Task<IActionResult> OnGetAsync()
    {
        PageNumber = Math.Max(1, PageNumber);

        UserName = User.Identity?.Name;
        Cheeps = await _service.GetCheepsAsync(PageNumber);
        if (User.Identity?.IsAuthenticated == true && UserName != null)
        {
            var user = await _service.findUserByName(UserName);
            if (user != null)
            {
                CurrentUser = user;

            }
            if (CurrentUser != null)
            {
                followedUsers = await _service.getFollowings(CurrentUser);
            }
        }
        return Page();
    }

    /// <summary>
    /// Handles POST requests for creating a new cheep.
    /// </summary>
    /// <param name="Input">The text content of the new cheep.</param>
    /// <returns>A redirect to the public timeline after successfully posting.</returns>
    public async Task<IActionResult> OnPostNewMessageAsync(String Input)
    {
        if (User.Identity?.IsAuthenticated == false || User.Identity?.Name == null)
        {
            return Page();
        }
        var user = await _service.findUserByName(User.Identity.Name);
        if (user == null)
        {
            return Page();
        }
        await _service.InsertCheepAsync(new CheepDTO
        {
            Text = Input,
            User = user,
            TimeStamp = DateTime.UtcNow
        });
        return RedirectToPage("Public");
    }

    /// <summary>
    /// Handles POST requests for following another user.
    /// </summary>
    /// <param name="followeeId">The ID of the user to follow.</param>
    /// <returns>A redirect to the public timeline after updating the follow list.</returns>
    public async Task<IActionResult> OnPostFollowAsync(string followeeId)
    {
        UserName = User.Identity?.Name;
        if (string.IsNullOrEmpty(UserName))
            return Unauthorized();

        var CurrentUser = await _service.findUserByName(UserName);
        if (CurrentUser == null) return Unauthorized();

        var ack = await _service.followUser(CurrentUser, followeeId);
        followedUsers = await _service.getFollowings(CurrentUser);
        return RedirectToPage("./Public");

    }

    /// <summary>
    /// Handles POST requests for unfollowing a user.
    /// </summary>
    /// <param name="unfolloweeId">The ID of the user to unfollow.</param>
    /// <returns>A redirect to the public timeline after the unfollow action.</returns>
    public async Task<IActionResult> OnPostUnfollowAsync(string unfolloweeId)
    {
        UserName = User.Identity?.Name;
        if (string.IsNullOrEmpty(UserName))
        {
            return Unauthorized();
        }

        var CurrentUser = await _service.findUserByName(UserName);
        if (CurrentUser == null)
        {
            return Unauthorized();
        }

        var ack = await _service.UnfollowUser(CurrentUser, unfolloweeId);
        followedUsers = await _service.getFollowings(CurrentUser);

        return RedirectToPage("./Public");
    }

    /// <summary>
    /// Handles POST requests for unliking a cheep.
    /// </summary>
    /// <param name="cheepId">The ID of the cheep to unlike.</param>
    /// <returns>A redirect to the public timeline after the unlike action.</returns>
    public async Task<IActionResult> OnPostUnLikeAsync(int cheepId)
    {
        UserName = User.Identity?.Name;
        if (string.IsNullOrEmpty(UserName))
        {
            return Unauthorized();
        }

        var CurrentUser = await _service.findUserByName(UserName);
        if (CurrentUser == null)
        {
            return Unauthorized();
        }

        var ack = await _service.UnLikeCheep(CurrentUser, cheepId);

        return RedirectToPage("./Public");
    }
    
    /// <summary>
    /// Handles POST requests for liking a cheep.
    /// </summary>
    /// <param name="cheepId">The ID of the cheep to like.</param>
    /// <returns>A redirect to the public timeline after the like action.</returns>
    public async Task<IActionResult> OnPostLikeAsync(int cheepId)
    {
        UserName = User.Identity?.Name;
        if (string.IsNullOrEmpty(UserName))
        {
            return Unauthorized();
        }

        var CurrentUser = await _service.findUserByName(UserName);
        if (CurrentUser == null)
        {
            return Unauthorized();
        }

        var ack = await _service.LikeCheep(CurrentUser, cheepId);

        return RedirectToPage("./Public");
    }
    
}