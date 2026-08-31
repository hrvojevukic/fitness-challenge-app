using System.Globalization;
using FitnessChallenge.Api.Dtos;
using FitnessChallenge.Api.Models;

namespace FitnessChallenge.Api.Services;

public class ActivityValidator : IActivityValidator
{
    private static readonly HashSet<Sport> DistanceSports = new() { Sport.Running, Sport.Walking, Sport.Cycling };
    private static readonly HashSet<Sport> DurationSports = new() { Sport.Gym, Sport.Swimming };

    private readonly IScoringService _scoringService;

    public ActivityValidator(IScoringService scoringService)
    {
        _scoringService = scoringService;
    }

    public ActivityValidationResult Validate(IngestActivityRequest request)
    {
        var result = new ActivityValidationResult();

        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            result.Errors.Add("userId is required.");
        }
        else if (!int.TryParse(request.UserId, out var userId) || userId <= 0)
        {
            result.Errors.Add("userId must be a valid user identifier.");
        }
        else
        {
            result.UserId = userId;
        }

        if (string.IsNullOrWhiteSpace(request.Datetime))
        {
            result.Errors.Add("datetime is required.");
        }
        else if (!DateTimeOffset.TryParse(
                     request.Datetime,
                     CultureInfo.InvariantCulture,
                     DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                     out var parsedDateTime))
        {
            result.Errors.Add("datetime must be a valid ISO 8601 date-time string.");
        }
        else
        {
            result.DateTimeUtc = parsedDateTime.UtcDateTime;
        }

        Sport? sport = null;
        if (!string.IsNullOrWhiteSpace(request.Sport))
        {
            if (!Enum.TryParse<Sport>(request.Sport, ignoreCase: true, out var parsedSport))
            {
                result.Errors.Add("sport must be one of: running, walking, cycling, gym, swimming.");
            }
            else
            {
                sport = parsedSport;
            }
        }

        result.Sport = sport;
        ValidateMetricsForSport(request, sport, result);

        return result;
    }

    private void ValidateMetricsForSport(IngestActivityRequest request, Sport? sport, ActivityValidationResult result)
    {
        if (sport is null)
        {
            if (request.Distance is not null || request.Duration is not null)
            {
                result.Errors.Add("Only 'steps' may be provided when 'sport' is omitted (Daily Steps entry).");
                return;
            }

            if (request.Steps is null)
            {
                result.Errors.Add("steps is required when sport is omitted.");
            }
            else if (request.Steps < 0)
            {
                result.Errors.Add("steps must be a non-negative integer.");
            }
            else
            {
                result.Steps = request.Steps;
            }

            return;
        }

        if (DistanceSports.Contains(sport.Value))
        {
            if (request.Steps is not null || request.Duration is not null)
            {
                result.Errors.Add($"Only 'distance' may be provided for sport '{sport}'.");
                return;
            }

            if (request.Distance is null)
            {
                result.Errors.Add("distance is required for this sport.");
            }
            else if (request.Distance < 0)
            {
                result.Errors.Add("distance must be a non-negative number.");
            }
            else
            {
                result.Distance = request.Distance;
            }

            return;
        }

        if (DurationSports.Contains(sport.Value))
        {
            if (request.Steps is not null || request.Distance is not null)
            {
                result.Errors.Add($"Only 'duration' may be provided for sport '{sport}'.");
                return;
            }

            if (string.IsNullOrWhiteSpace(request.Duration))
            {
                result.Errors.Add("duration is required for this sport.");
            }
            else if (!_scoringService.TryParseDurationMinutes(request.Duration, out _))
            {
                result.Errors.Add("duration must be in 'minutes:seconds' format, e.g. '12:34'.");
            }
            else
            {
                result.Duration = request.Duration;
            }
        }
    }
}
