using FitnessChallenge.Api.Extensions;
using FitnessChallenge.Application.Dtos;
using FitnessChallenge.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessChallenge.Api.Controllers;

[ApiController]
[Route("api/me")]
[Authorize]
[Produces("application/json")]
public class MeController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public MeController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>Returns the signed-in user's dashboard.</summary>
    [HttpGet("dashboard")]
    [ProducesResponseType<DashboardResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DashboardResponseDto>> GetDashboard()
    {
        return Ok(await _dashboardService.BuildForUserAsync(User.GetUserId()));
    }
}
