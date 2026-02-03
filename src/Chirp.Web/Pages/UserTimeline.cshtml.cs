using Chirp.Domain;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Razor.Pages;

/// <summary>
/// Represents the timeline page for a specific user in Chirp.
/// Displays the user's own cheeps and, if viewing their own profile,
/// also includes cheeps from users they follow.
/// </summary>
public class UserTimelineModel : PageModel
{
    private readonly ICheepService _service;
    public List<CheepDTO> Cheeps { get; set; } = new();

    public List<CheepDTO> CheepsFromFollowings { get; set; } = new();
    public string? UserName { get; private set; }

    [BindProperty(SupportsGet = true, Name = "pagenumber")]
    public int PageNumber { get; set; } = 1;

    public string? AuthorName { get; set; }

    public User? CurrentUser { get; set; }

    public List<string> followedUsers { get; set; } = new();

    public UserTimelineModel(ICheepService service)
    {
        _service = service;
    }

    /// <summary>
    /// Handles GET requests to display a user's timeline.
    /// Loads the cheeps authored by the target user, and if the current user
    /// is viewing their own timeline, also loads cheeps from their followings.
    /// </summary>
    /// <param name="author">The username of the timeline author.</param>
    /// <returns>The <see cref="IActionResult"/> representing the rendered timeline page.</returns>
    public async Task<IActionResult> OnGet(string author)
{
    UserName = User.Identity?.Name;
    AuthorName = author;
    PageNumber = Math.Max(1, PageNumber);
    var timelineUser = await _service.findUserByName(author);

    if (timelineUser != null)
        {
            Cheeps = await _service.getCheepsFromUser(timelineUser, PageNumber);
        }

    if (User.Identity?.IsAuthenticated == true && UserName != null)
    {
        CurrentUser = await _service.findUserByName(UserName);

        if (CurrentUser != null && timelineUser != null &&
            CurrentUser.Id == timelineUser.Id)
        {
            followedUsers = await _service.getFollowings(CurrentUser);

            if (followedUsers != null)
            {
                foreach (var userId in followedUsers)
                {
                    var tempCheeps = await _service.GetCheepsFromUserId(userId, PageNumber);
                    if (tempCheeps != null)
                    {
                        CheepsFromFollowings.AddRange(tempCheeps);
                    }
                }
            }
        }
    }

    return Page();
}

    /// <summary>
    /// Handles POST requests for unfollowing another user from their timeline page.
    /// </summary>
    /// <param name="unfolloweeId">The ID of the user to unfollow.</param>
    /// <returns>A redirect to the updated timeline page.</returns>
    public async Task<IActionResult> OnPostUnfollowAsync(string unfolloweeId)
    {
        UserName= User.Identity?.Name;
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

        return RedirectToPage();
    }
    
}

