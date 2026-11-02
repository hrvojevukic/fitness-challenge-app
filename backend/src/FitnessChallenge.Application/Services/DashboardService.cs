using FitnessChallenge.Application.Abstractions;
using FitnessChallenge.Application.Dtos;
using FitnessChallenge.Application.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FitnessChallenge.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IAppDbContext _db;

    public DashboardService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardResponseDto> BuildForUserAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null)
        {
            throw new NotFoundException($"No user exists with id '{userId}'.");
        }

        var sportBreakdown = await BuildSportBreakdownAsync(userId);

        return new DashboardResponseDto
        {
            UserId = user.Id,
            FullName = $"{user.FirstName} {user.LastName}",
            TotalPoints = sportBreakdown.Sum(row => row.TotalPoints),
            ActivityCount = sportBreakdown.Sum(row => row.ActivityCount),
            SportBreakdown = sportBreakdown,
            DailyVolume = await BuildDailyVolumeAsync(userId),
        };
    }

    private async Task<List<SportBreakdownDto>> BuildSportBreakdownAsync(int userId)
    {
        var groups = await _db.Activities
            .Where(a => a.UserId == userId)
            .GroupBy(a => a.Sport)
            .Select(g => new
            {
                Sport = g.Key,
                ActivityCount = g.Count(),
                TotalPoints = g.Sum(a => a.Points),
            })
            .ToListAsync();

        var breakdown = new List<SportBreakdownDto>();
        foreach (var group in groups)
        {
            breakdown.Add(new SportBreakdownDto
            {
                Label = group.Sport?.ToString() ?? "Daily Steps",
                ActivityCount = group.ActivityCount,
                TotalPoints = group.TotalPoints,
            });
        }

        return breakdown
            .OrderByDescending(row => row.TotalPoints)
            .ToList();
    }

    private async Task<List<DailyVolumeDto>> BuildDailyVolumeAsync(int userId)
    {
        var groups = await _db.Activities
            .Where(a => a.UserId == userId)
            .GroupBy(a => a.DateTimeUtc.Date)
            .Select(g => new
            {
                Day = g.Key,
                Points = g.Sum(a => a.Points),
                ActivityCount = g.Count(),
            })
            .OrderBy(g => g.Day)
            .ToListAsync();

        var dailyVolume = new List<DailyVolumeDto>();
        foreach (var group in groups)
        {
            dailyVolume.Add(new DailyVolumeDto
            {
                Date = DateOnly.FromDateTime(group.Day),
                Points = group.Points,
                ActivityCount = group.ActivityCount,
            });
        }

        return dailyVolume;
    }
}
