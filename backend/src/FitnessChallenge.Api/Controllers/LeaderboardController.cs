using FitnessChallenge.Application.Dtos;
using FitnessChallenge.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessChallenge.Api.Controllers;

[ApiController]
[Route("api/leaderboard")]
[Produces("application/json")]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _leaderboardService;

    public LeaderboardController(ILeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }

    /// <summary>Returns one page of the global ranking by total points.</summary>
    /// <remarks>The trend compares each user's rank now with their rank based only on activities at least 7 days old.</remarks>
    [HttpGet]
    [ProducesResponseType<PagedResult<LeaderboardEntryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<LeaderboardEntryDto>>> Get([FromQuery] PageQuery paging)
    {
        return Ok(await _leaderboardService.GetAsync(paging));
    }
}
