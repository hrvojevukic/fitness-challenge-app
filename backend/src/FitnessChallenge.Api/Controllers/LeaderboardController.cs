using FitnessChallenge.Api.Data;
using FitnessChallenge.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessChallenge.Api.Controllers;

[ApiController]
[Route("api/leaderboard")]
public class LeaderboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public LeaderboardController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<LeaderboardEntryDto>>> Get()
    {
        var users = await _db.Users.ToListAsync();
        var activities = await _db.Activities.ToListAsync();

        var cutoff = DateTime.UtcNow.AddDays(-7);

        var totalPoints = new Dictionary<int, int>();
        var pointsBeforeCutoff = new Dictionary<int, int>();
        var hadActivityBeforeCutoff = new Dictionary<int, bool>();

        foreach (var user in users)
        {
            totalPoints[user.Id] = 0;
            pointsBeforeCutoff[user.Id] = 0;
            hadActivityBeforeCutoff[user.Id] = false;
        }

        foreach (var activity in activities)
        {
            if (!totalPoints.ContainsKey(activity.UserId))
            {
                continue;
            }

            totalPoints[activity.UserId] += activity.Points;

            if (activity.DateTimeUtc <= cutoff)
            {
                pointsBeforeCutoff[activity.UserId] += activity.Points;
                hadActivityBeforeCutoff[activity.UserId] = true;
            }
        }

        var currentRanks = RankByPoints(totalPoints);
        var previousRanks = RankByPoints(pointsBeforeCutoff);

        var entries = new List<LeaderboardEntryDto>();
        foreach (var user in users)
        {
            var currentRank = currentRanks[user.Id];
            int? previousRank = hadActivityBeforeCutoff[user.Id] ? previousRanks[user.Id] : null;

            string trend;
            if (previousRank is null)
            {
                trend = "new";
            }
            else if (previousRank.Value == currentRank)
            {
                trend = "same";
            }
            else if (previousRank.Value > currentRank)
            {
                trend = "up";
            }
            else
            {
                trend = "down";
            }

            entries.Add(new LeaderboardEntryDto
            {
                UserId = user.Id,
                FullName = $"{user.FirstName} {user.LastName}",
                TotalPoints = totalPoints[user.Id],
                Rank = currentRank,
                PreviousRank = previousRank,
                Trend = trend,
            });
        }

        return Ok(entries.OrderBy(entry => entry.Rank).ToList());
    }

    private static Dictionary<int, int> RankByPoints(Dictionary<int, int> pointsByUserId)
    {
        var ordered = pointsByUserId
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => pair.Key)
            .ToList();

        var ranks = new Dictionary<int, int>();
        for (var i = 0; i < ordered.Count; i++)
        {
            ranks[ordered[i].Key] = i + 1;
        }

        return ranks;
    }
}
