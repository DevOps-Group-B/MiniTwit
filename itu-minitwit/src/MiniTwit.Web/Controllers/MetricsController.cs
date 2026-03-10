using Chirp.Core.Models;
using Chirp.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minitwit.Services;

namespace Chirp.Web.Controllers;

[ApiController]
[Route("")]
public class MetricsController : ControllerBase
{
    private readonly IMetricsService _metrics;
    private readonly UserManager<Author> _userManager;
    private readonly ICheepService _cheepService;

    public MetricsController(IMetricsService metrics, UserManager<Author> userManager, ICheepService cheepService)
    {
        _metrics = metrics;
        _userManager = userManager;
        _cheepService = cheepService;
    }

    [HttpGet("total-users")]
    public async Task<IActionResult> GetTotalUsers()
    {
        var totalUsers = await _userManager.Users.CountAsync();

        _metrics.SetTotalUsers(totalUsers);

        return Ok(new { TotalUsers = totalUsers });
    }

    [HttpGet("total-cheeps")]
    public async Task<IActionResult> GetTotalCheeps()
    {
        var totalCheeps = await _cheepService.GetTotalCheepsAsync();
        _metrics.SetTotalCheeps(totalCheeps);
        return Ok(new { TotalCheeps = totalCheeps });
    }
}