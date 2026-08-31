using FitnessChallenge.Api.Data;
using FitnessChallenge.Api.Dtos;
using FitnessChallenge.Api.Models;
using FitnessChallenge.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessChallenge.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IDashboardService _dashboardService;

    public UsersController(AppDbContext db, IDashboardService dashboardService)
    {
        _db = db;
        _dashboardService = dashboardService;
    }

    [HttpPost]
    public async Task<ActionResult<RegisterUserResponse>> Register(RegisterUserRequest request)
    {
        var firstName = request.FirstName?.Trim();
        var lastName = request.LastName?.Trim();

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            return ValidationProblem("firstName and lastName are both required.");
        }

        var alreadyExists = await _db.Users.AnyAsync(u =>
            u.FirstName.ToLower() == firstName.ToLower() && u.LastName.ToLower() == lastName.ToLower());

        if (alreadyExists)
        {
            return Conflict(new { message = $"A user named '{firstName} {lastName}' is already registered." });
        }

        var user = new User { FirstName = firstName, LastName = lastName };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var response = ToResponse(user);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, response);
    }

    [HttpGet]
    public async Task<ActionResult<List<RegisterUserResponse>>> GetAll()
    {
        var users = await _db.Users
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync();

        var response = new List<RegisterUserResponse>();
        foreach (var user in users)
        {
            response.Add(ToResponse(user));
        }

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RegisterUserResponse>> GetById(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(user));
    }

    [HttpGet("{id:int}/dashboard")]
    public async Task<ActionResult<DashboardResponseDto>> GetDashboard(int id)
    {
        var dashboard = await _dashboardService.BuildForUserAsync(id);
        if (dashboard is null)
        {
            return NotFound();
        }

        return Ok(dashboard);
    }

    private static RegisterUserResponse ToResponse(User user)
    {
        return new RegisterUserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
        };
    }

    private ActionResult ValidationProblem(string message)
    {
        return BadRequest(new ProblemDetails { Title = "Invalid request", Detail = message, Status = 400 });
    }
}
