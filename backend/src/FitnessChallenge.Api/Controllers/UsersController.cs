using FitnessChallenge.Application.Dtos;
using FitnessChallenge.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessChallenge.Api.Controllers;

[ApiController]
[Route("api/users")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IDashboardService _dashboardService;

    public UsersController(IUserService userService, IDashboardService dashboardService)
    {
        _userService = userService;
        _dashboardService = dashboardService;
    }

    /// <summary>Registers a user. The first and last name must be unique together.</summary>
    [HttpPost]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Register(RegisterUserRequest request)
    {
        var user = await _userService.RegisterAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    /// <summary>Returns one page of users, ordered by name.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<UserResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<UserResponse>>> GetPage([FromQuery] PageQuery paging)
    {
        return Ok(await _userService.GetPageAsync(paging));
    }

    /// <summary>Returns one user.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(int id)
    {
        return Ok(await _userService.GetByIdAsync(id));
    }

    /// <summary>Returns a user's total points, sport breakdown and daily activity volume.</summary>
    [HttpGet("{id:int}/dashboard")]
    [ProducesResponseType<DashboardResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DashboardResponseDto>> GetDashboard(int id)
    {
        return Ok(await _dashboardService.BuildForUserAsync(id));
    }
}
