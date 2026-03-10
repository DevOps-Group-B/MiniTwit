using Chirp.Core.DTOs.Simulator;
using Chirp.Core.Models;
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

    public MetricsController(IMetricsService metrics, UserManager<Author> userManager)
    {
        _metrics = metrics;
        _userManager = userManager;
    }

    [HttpGet("total-users")]
    public async Task<IActionResult> GetTotalUsers()
    {
        var totalUsers = await _userManager.Users.CountAsync();

        _metrics.SetTotalUsers(totalUsers);

        return Ok(new { TotalUsers = totalUsers });
    }
}