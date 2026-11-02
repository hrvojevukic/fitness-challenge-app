using FitnessChallenge.Application.Abstractions;
using FitnessChallenge.Application.Dtos;
using FitnessChallenge.Application.Exceptions;
using FitnessChallenge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessChallenge.Application.Services;

public interface IActivityService
{
    Task<ActivityResponse> IngestAsync(IngestActivityRequest request, int callerId);
    Task DeleteAsync(int activityId, int callerId);
    Task<PagedResult<ActivityResponse>> GetForUserAsync(int userId, PageQuery paging);
}

public class ActivityService : IActivityService
{
    private readonly IAppDbContext _db;
    private readonly IActivityValidator _validator;
    private readonly IScoringService _scoringService;

    public ActivityService(IAppDbContext db, IActivityValidator validator, IScoringService scoringService)
    {
        _db = db;
        _validator = validator;
        _scoringService = scoringService;
    }

    public async Task<ActivityResponse> IngestAsync(IngestActivityRequest request, int callerId)
    {
        if (!string.IsNullOrWhiteSpace(request.UserId)
            && (!int.TryParse(request.UserId, out var bodyUserId) || bodyUserId != callerId))
        {
            throw new ForbiddenException("You can only log activities for yourself.");
        }

        request.UserId = callerId.ToString();

        var validation = _validator.Validate(request);
        if (!validation.IsValid)
        {
            throw new InvalidRequestException(string.Join(" ", validation.Errors));
        }

        var userExists = await _db.Users.AnyAsync(u => u.Id == validation.UserId);
        if (!userExists)
        {
            throw new InvalidRequestException($"No user exists with id '{validation.UserId}'.");
        }

        var activity = new Activity
        {
            UserId = validation.UserId,
            DateTimeUtc = validation.DateTimeUtc,
            Sport = validation.Sport,
            Steps = validation.Steps,
            Distance = validation.Distance,
            Duration = validation.Duration,
            Points = _scoringService.CalculatePoints(validation.Sport, validation.Distance, validation.Duration, validation.Steps),
        };

        _db.Activities.Add(activity);
        await _db.SaveChangesAsync();

        return activity.ToResponse();
    }

    public async Task DeleteAsync(int activityId, int callerId)
    {
        var activity = await _db.Activities.FindAsync(activityId);
        if (activity is null)
        {
            throw new NotFoundException($"No activity exists with id '{activityId}'.");
        }

        if (activity.UserId != callerId)
        {
            throw new ForbiddenException("You can only delete your own activities.");
        }

        _db.Activities.Remove(activity);
        await _db.SaveChangesAsync();
    }

    public Task<PagedResult<ActivityResponse>> GetForUserAsync(int userId, PageQuery paging)
    {
        return _db.Activities
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.DateTimeUtc)
            .ThenByDescending(a => a.Id)
            .ToPagedResultAsync(paging, activity => activity.ToResponse());
    }
}
