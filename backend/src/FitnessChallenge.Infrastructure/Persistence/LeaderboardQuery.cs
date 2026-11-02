using FitnessChallenge.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace FitnessChallenge.Infrastructure.Persistence;

public class LeaderboardQuery : ILeaderboardQuery
{
    private readonly AppDbContext _db;

    public LeaderboardQuery(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<LeaderboardRow>> GetPageAsync(DateTime cutoff, int skip, int take)
    {
        return _db.Database.SqlQuery<LeaderboardRow>($"""
            WITH Totals AS (
                SELECT
                    u.Id AS UserId,
                    u.FirstName,
                    u.LastName,
                    COALESCE(SUM(a.Points), 0) AS TotalPoints,
                    COALESCE(SUM(CASE WHEN a.DateTimeUtc <= {cutoff} THEN a.Points END), 0) AS PointsBeforeCutoff,
                    COUNT(CASE WHEN a.DateTimeUtc <= {cutoff} THEN 1 END) AS ActivitiesBeforeCutoff
                FROM Users u
                LEFT JOIN Activities a ON a.UserId = u.Id
                GROUP BY u.Id, u.FirstName, u.LastName
            )
            SELECT
                UserId,
                FirstName,
                LastName,
                TotalPoints,
                ActivitiesBeforeCutoff,
                ROW_NUMBER() OVER (ORDER BY TotalPoints DESC, UserId) AS CurrentRank,
                ROW_NUMBER() OVER (ORDER BY PointsBeforeCutoff DESC, UserId) AS PreviousRank
            FROM Totals
            ORDER BY CurrentRank
            LIMIT {take} OFFSET {skip}
            """).ToListAsync();
    }
}
