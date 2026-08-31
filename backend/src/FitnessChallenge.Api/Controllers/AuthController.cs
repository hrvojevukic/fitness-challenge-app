using System.Security.Claims;
using FitnessChallenge.Api.Data;
using FitnessChallenge.Api.Dtos;
using FitnessChallenge.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessChallenge.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;

    public AuthController(AppDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var firstName = request.FirstName?.Trim();
        var lastName = request.LastName?.Trim();

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid request",
                Detail = "firstName and lastName are both required.",
                Status = 400,
            });
        }

        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.FirstName.ToLower() == firstName.ToLower() && u.LastName.ToLower() == lastName.ToLower());

        if (user is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Login failed",
                Detail = $"No registered user named '{firstName} {lastName}'.",
                Status = 401,
            });
        }

        return Ok(new LoginResponse
        {
            Token = _tokenService.CreateToken(user),
            User = new RegisterUserResponse { Id = user.Id, FirstName = user.FirstName, LastName = user.LastName },
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<RegisterUserResponse>> Me()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _db.Users.FindAsync(userId);
        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(new RegisterUserResponse { Id = user.Id, FirstName = user.FirstName, LastName = user.LastName });
    }
}
