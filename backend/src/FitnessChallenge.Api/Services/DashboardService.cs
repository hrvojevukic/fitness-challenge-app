using FitnessChallenge.Api.Data;
using FitnessChallenge.Api.Dtos;
using FitnessChallenge.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessChallenge.Api.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardResponseDto?> BuildForUserAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null)
        {
            return null;
        }

        var activities = await _db.Activities
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.DateTimeUtc)
            .ToListAsync();

        var response = new DashboardResponseDto
        {
            UserId = user.Id,
            FullName = $"{user.FirstName} {user.LastName}",
        };

        var totalPoints = 0;
        foreach (var activity in activities)
        {
            totalPoints += activity.Points;
            response.Activities.Add(MapActivity(activity));
        }

        response.TotalPoints = totalPoints;
        response.SportBreakdown = BuildSportBreakdown(activities);
        response.DailyVolume = BuildDailyVolume(activities);

        return response;
    }

    private static List<SportBreakdownDto> BuildSportBreakdown(List<Activity> activities)
    {
        var bySport = new Dictionary<string, SportBreakdownDto>();

        foreach (var activity in activities)
        {
            var label = activity.Sport?.ToString() ?? "Daily Steps";

            if (!bySport.TryGetValue(label, out var row))
            {
                row = new SportBreakdownDto { Label = label };
                bySport[label] = row;
            }

            row.ActivityCount += 1;
            row.TotalPoints += activity.Points;
        }

        return bySport.Values
            .OrderByDescending(row => row.TotalPoints)
            .ToList();
    }

    private static List<DailyVolumeDto> BuildDailyVolume(List<Activity> activities)
    {
        var byDay = new Dictionary<DateOnly, DailyVolumeDto>();

        foreach (var activity in activities)
        {
            var day = DateOnly.FromDateTime(activity.DateTimeUtc);

            if (!byDay.TryGetValue(day, out var row))
            {
                row = new DailyVolumeDto { Date = day };
                byDay[day] = row;
            }

            row.ActivityCount += 1;
            row.Points += activity.Points;
        }

        return byDay.Values
            .OrderBy(row => row.Date)
            .ToList();
    }

    private static ActivityResponse MapActivity(Activity activity)
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
