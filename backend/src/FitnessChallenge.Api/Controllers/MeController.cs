using System.Security.Claims;
using FitnessChallenge.Api.Dtos;
using FitnessChallenge.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessChallenge.Api.Controllers;

[ApiController]
[Route("api/me")]
[Authorize]
public class MeController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public MeController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardResponseDto>> GetDashboard()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Unauthorized();
        }

        var dashboard = await _dashboardService.BuildForUserAsync(userId);
        return dashboard is null ? NotFound() : Ok(dashboard);
    }
}
