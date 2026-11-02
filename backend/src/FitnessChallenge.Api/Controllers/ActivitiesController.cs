using FitnessChallenge.Api.Extensions;
using FitnessChallenge.Application.Dtos;
using FitnessChallenge.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessChallenge.Api.Controllers;

[ApiController]
[Route("api/activities")]
[Produces("application/json")]
public class ActivitiesController : ControllerBase
{
    private readonly IActivityService _activityService;

    public ActivitiesController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    /// <summary>Logs an activity for the signed-in user and awards its points.</summary>
    /// <remarks>
    /// Running, walking and cycling take a distance in kilometers. Gym and swimming take a
    /// minutes:seconds duration. Leave the sport out and send steps for a Daily Steps entry.
    /// </remarks>
    [Authorize]
    [HttpPost]
    [ProducesResponseType<ActivityResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ActivityResponse>> Ingest(IngestActivityRequest request)
    {
        var activity = await _activityService.IngestAsync(request, User.GetUserId());
        return CreatedAtAction(nameof(GetForUser), new { userId = activity.UserId }, activity);
    }

    /// <summary>Deletes one of the signed-in user's activities.</summary>
    [Authorize]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _activityService.DeleteAsync(id, User.GetUserId());
        return NoContent();
    }

    /// <summary>Returns one page of a user's activities, newest first.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<ActivityResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<ActivityResponse>>> GetForUser([FromQuery] int userId, [FromQuery] PageQuery paging)
    {
        return Ok(await _activityService.GetForUserAsync(userId, paging));
    }
}
