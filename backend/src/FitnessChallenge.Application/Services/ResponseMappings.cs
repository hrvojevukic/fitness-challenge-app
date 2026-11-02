using FitnessChallenge.Application.Dtos;
using FitnessChallenge.Domain.Entities;

namespace FitnessChallenge.Application.Services;

internal static class ResponseMappings
{
    public static UserResponse ToResponse(this User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
        };
    }

    public static ActivityResponse ToResponse(this Activity activity)
    {
        return new ActivityResponse
        {
            Id = activity.Id,
            UserId = activity.UserId,
            Datetime = DateTime.SpecifyKind(activity.DateTimeUtc, DateTimeKind.Utc),
            Sport = activity.Sport?.ToString().ToLowerInvariant(),
            Steps = activity.Steps,
            Distance = activity.Distance,
            Duration = activity.Duration,
            Points = activity.Points,
        };
    }
}
