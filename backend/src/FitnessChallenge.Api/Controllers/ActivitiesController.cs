using System.Security.Claims;
using FitnessChallenge.Api.Data;
using FitnessChallenge.Api.Dtos;
using FitnessChallenge.Api.Models;
using FitnessChallenge.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessChallenge.Api.Controllers;

[ApiController]
[Route("api/activities")]
public class ActivitiesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IActivityValidator _validator;
    private readonly IScoringService _scoringService;

    public ActivitiesController(AppDbContext db, IActivityValidator validator, IScoringService scoringService)
    {
        _db = db;
        _validator = validator;
        _scoringService = scoringService;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ActivityResponse>> Ingest(IngestActivityRequest request)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var callerId))
        {
            return Unauthorized();
        }

        if (!string.IsNullOrWhiteSpace(request.UserId)
            && (!int.TryParse(request.UserId, out var bodyUserId) || bodyUserId != callerId))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Title = "Forbidden",
                Status = 403,
                Detail = "You can only log activities for yourself.",
            });
        }

        request.UserId = callerId.ToString();

        var validation = _validator.Validate(request);
        if (!validation.IsValid)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid request",
                Status = 400,
                Detail = string.Join(" ", validation.Errors),
            });
        }

        var userExists = await _db.Users.AnyAsync(u => u.Id == validation.UserId);
        if (!userExists)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid request",
                Status = 400,
                Detail = $"No user exists with id '{validation.UserId}'.",
            });
        }

        var points = _scoringService.CalculatePoints(validation.Sport, validation.Distance, validation.Duration, validation.Steps);

        var activity = new Activity
        {
            UserId = validation.UserId,
            DateTimeUtc = validation.DateTimeUtc,
            Sport = validation.Sport,
            Steps = validation.Steps,
            Distance = validation.Distance,
            Duration = validation.Duration,
            Points = points,
        };

        _db.Activities.Add(activity);
        await _db.SaveChangesAsync();

        var response = ToResponse(activity);
        return CreatedAtAction(nameof(GetForUser), new { userId = activity.UserId }, response);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var callerId))
        {
            return Unauthorized();
        }

        var activity = await _db.Activities.FindAsync(id);
        if (activity is null)
        {
            return NotFound();
        }

        if (activity.UserId != callerId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Title = "Forbidden",
                Status = 403,
                Detail = "You can only delete your own activities.",
            });
        }

        _db.Activities.Remove(activity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<List<ActivityResponse>>> GetForUser([FromQuery] int userId)
    {
        var activities = await _db.Activities
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.DateTimeUtc)
            .ToListAsync();

        var response = new List<ActivityResponse>();
        foreach (var activity in activities)
        {
            response.Add(ToResponse(activity));
        }

        return Ok(response);
    }

    private static ActivityResponse ToResponse(Activity activity)
    {
        return new ActivityResponse
        {
            Id = activity.Id,
            UserId = activity.UserId,
            Datetime = activity.DateTimeUtc,
            Sport = activity.Sport?.ToString().ToLowerInvariant(),
            Steps = activity.Steps,
            Distance = activity.Distance,
            Duration = activity.Duration,
            Points = activity.Points,
        };
    }
}
