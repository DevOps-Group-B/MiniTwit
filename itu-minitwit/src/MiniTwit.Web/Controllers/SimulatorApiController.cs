using Chirp.Core.DTOs.Simulator;
using Chirp.Core.Models;
using Chirp.Core.Services;
using Chirp.Core.Simulator;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chirp.Web.Controllers;

[ApiController]
[Route("")]
public class SimulatorApiController : ControllerBase
{
    private readonly ISimulatorRepository _simRepo;
    private readonly ICheepService _cheepService;
    private readonly IAuthorService _authorService;
    private readonly UserManager<Author> _userManager;

    public SimulatorApiController(ISimulatorRepository simRepo, ICheepService cheepService, IAuthorService authorService, UserManager<Author> userManager)
    {
        _simRepo = simRepo;
        _cheepService = cheepService;
        _authorService = authorService;
        _userManager = userManager;
    }

    // Helper: Updates the 'latest' value if provided in the query string
    private async Task UpdateLatest(int? latest)
    {
        if (latest.HasValue)
        {
            await _simRepo.UpdateLatestAsync(latest.Value);
        }
    }

    // Helper: Checks for the specific Simulator Authorization header
    private bool IsAuthorized()
    {
        // "Basic c2ltdWxhdG9yOnN1cGVyX3NhZmUh" decodes to "simulator:super_safe!"
        // The simulator sends this specific header.
        return Request.Headers["Authorization"] == "Basic c2ltdWxhdG9yOnN1cGVyX3NhZmUh";
    }

    // -------------------------------------------------------------------
    // 1. LATEST
    // -------------------------------------------------------------------

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest()
    {
        var val = await _simRepo.GetLatestAsync();
        return Ok(new LatestValueDTO { Latest = val });
    }

    // -------------------------------------------------------------------
    // 2. REGISTER
    // -------------------------------------------------------------------

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDTO payload, [FromQuery] int? latest)
    {
        await UpdateLatest(latest);

        string error = null;

        if (string.IsNullOrEmpty(payload.Username)) error = "You have to enter a username";
        else if (string.IsNullOrEmpty(payload.Email) || !payload.Email.Contains("@")) error = "You have to enter a valid email address";
        else if (string.IsNullOrEmpty(payload.Pwd)) error = "You have to enter a password";
        
        var user = new Author
        {
            UserName = payload.Username,
            Email = payload.Email,
            Name = payload.Username,
            Cheeps = new List<Cheep>(),
            Followers = new List<AuthorFollower>(),
            Following = new List<AuthorFollower>()
        };
        
        var result = await _userManager.CreateAsync(user, payload.Pwd);
        
        if (!result.Succeeded)
        {
            error = string.Join(", ", result.Errors.Select(e => e.Description));
        }

        if (error != null)
        {
            return BadRequest(new ErrorResponseDTO { Status = 400, ErrorMsg = error });
        }

        return NoContent(); // 204 No Content
    }

    // -------------------------------------------------------------------
    // 3. MESSAGES
    // -------------------------------------------------------------------

    [HttpGet("msgs")]
    public async Task<IActionResult> GetPublicMessages([FromQuery] int no = 100, [FromQuery] int? latest = null)
    {
        await UpdateLatest(latest);
        
        if (!IsAuthorized()) 
            return StatusCode(403, new ErrorResponseDTO { Status = 403, ErrorMsg = "You are not authorized to use this resource!" });
        
        var (messages, _) = await _cheepService.GetCheepsAmountAsync(no);

        var dtos = new List<MessageDTO>();
        
        foreach(var m in messages) {
            dtos.Add(new MessageDTO {
                Content = m.Message,
                PubDate = m.Timestamp.ToString("yyyy-MM-dd HH':'mm':'ss"),
                User = m.AuthorName
            });
        }
        
        return Ok(dtos); 
    }

    [HttpGet("msgs/{username}")]
    public async Task<IActionResult> GetUserMessages(string username, [FromQuery] int no = 100, [FromQuery] int? latest = null)
    {
        await UpdateLatest(latest);
        
        if (!IsAuthorized()) 
            return StatusCode(403, new ErrorResponseDTO { Status = 403, ErrorMsg = "You are not authorized to use this resource!" });
        
        var user = await _authorService.GetAuthorByNameAsync(username);
        if (user == null) return NotFound();
        
        var (messages, _) = await _cheepService.GetCheepsFromAuthorAmountAsync(no, user.Id);
        
        var dtos = new List<MessageDTO>();
        
        foreach(var m in messages) {
            dtos.Add(new MessageDTO {
                Content = m.Message,
                PubDate = m.Timestamp.ToString("yyyy-MM-dd HH':'mm':'ss"),
                User = m.AuthorName
            });
        }

        return Ok(dtos);
    }

    [HttpPost("msgs/{username}")]
    public async Task<IActionResult> PostMessage(string username, [FromBody] PostMessageDTO payload, [FromQuery] int? latest = null)
    {
        await UpdateLatest(latest);
        
        if (!IsAuthorized()) 
            return StatusCode(403, new ErrorResponseDTO { Status = 403, ErrorMsg = "You are not authorized to use this resource!" });
        
        var user = await _authorService.GetAuthorByNameAsync(username);
        if (user == null) return NotFound();

        var cheep = new Cheep
        {
            Text = payload.Content,
            AuthorId = user.Id,
            TimeStamp = DateTime.Now
        };
        
        await _cheepService.PostCheepAsync(cheep);

        return NoContent(); // 204 No Content
    }

    // -------------------------------------------------------------------
    // 4. FOLLOWS
    // -------------------------------------------------------------------

    [HttpGet("fllws/{username}")]
    public async Task<IActionResult> GetFollowers(string username, [FromQuery] int no = 100, [FromQuery] int? latest = null)
    {
        await UpdateLatest(latest);
        
        if (!IsAuthorized()) 
            return StatusCode(403, new ErrorResponseDTO { Status = 403, ErrorMsg = "You are not authorized to use this resource!" });
        
        var user = await _authorService.GetAuthorByNameAsync(username);
        if (user == null) return NotFound();
        
        var followingNames = await _authorService.GetFollowingAmountAsync(no, user.Id);
        
        return Ok(new FollowsResponseDTO { Follows = followingNames });
    }

    [HttpPost("fllws/{username}")]
    public async Task<IActionResult> FollowUser(string username, [FromBody] FollowActionDTO payload, [FromQuery] int? latest = null)
    {
        await UpdateLatest(latest);
        
        if (!IsAuthorized()) 
            return StatusCode(403, new ErrorResponseDTO { Status = 403, ErrorMsg = "You are not authorized to use this resource!" });

        var user = await _authorService.GetAuthorByNameAsync(username);
        if (user == null) return NotFound();

        if (!string.IsNullOrEmpty(payload.Follow))
        {
            var userToFollow = await _authorService.GetAuthorByNameAsync(payload.Follow);
            if (userToFollow == null) return NotFound();
            
            await _authorService.FollowAuthorAsync(user.Id, userToFollow.Id);
        }
        else if (!string.IsNullOrEmpty(payload.Unfollow))
        {
            var userToUnfollow = await _authorService.GetAuthorByNameAsync(payload.Unfollow);
            if (userToUnfollow == null) return NotFound();
            
            await _authorService.UnfollowAuthorAsync(user.Id, userToUnfollow.Id);
        }

        return NoContent();
    }
}