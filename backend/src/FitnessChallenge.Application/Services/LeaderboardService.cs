using FitnessChallenge.Application.Abstractions;
using FitnessChallenge.Application.Dtos;
using Microsoft.EntityFrameworkCore;

namespace FitnessChallenge.Application.Services;

public interface ILeaderboardService
{
    Task<PagedResult<LeaderboardEntryDto>> GetAsync(PageQuery paging);
}

public class LeaderboardService : ILeaderboardService
{
    private const int TrendWindowDays = 7;

    private readonly IAppDbContext _db;
    private readonly ILeaderboardQuery _leaderboardQuery;

    public LeaderboardService(IAppDbContext db, ILeaderboardQuery leaderboardQuery)
    {
        _db = db;
        _leaderboardQuery = leaderboardQuery;
    }

    public async Task<PagedResult<LeaderboardEntryDto>> GetAsync(PageQuery paging)
    {
        var cutoff = DateTime.UtcNow.AddDays(-TrendWindowDays);
        var rows = await _leaderboardQuery.GetPageAsync(cutoff, paging.Skip, paging.PageSize);

        var entries = new List<LeaderboardEntryDto>();
        foreach (var row in rows)
        {
            int? previousRank = row.ActivitiesBeforeCutoff > 0 ? row.PreviousRank : null;

            entries.Add(new LeaderboardEntryDto
            {
                UserId = row.UserId,
                FullName = $"{row.FirstName} {row.LastName}",
                TotalPoints = row.TotalPoints,
                Rank = row.CurrentRank,
                PreviousRank = previousRank,
                Trend = GetTrend(previousRank, row.CurrentRank),
            });
        }

        return new PagedResult<LeaderboardEntryDto>
        {
            Items = entries,
            Page = paging.Page,
            PageSize = paging.PageSize,
            TotalCount = await _db.Users.CountAsync(),
        };
    }

    private static string GetTrend(int? previousRank, int currentRank)
    {
        if (previousRank is null)
        {
            return "new";
        }

        if (previousRank.Value == currentRank)
        {
            return "same";
        }

        return previousRank.Value > currentRank ? "up" : "down";
    }
}
