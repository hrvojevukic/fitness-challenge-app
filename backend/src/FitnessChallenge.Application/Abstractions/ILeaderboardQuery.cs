namespace FitnessChallenge.Application.Abstractions;

public class LeaderboardRow
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int TotalPoints { get; set; }
    public int CurrentRank { get; set; }
    public int PreviousRank { get; set; }
    public int ActivitiesBeforeCutoff { get; set; }
}

public interface ILeaderboardQuery
{
    Task<List<LeaderboardRow>> GetPageAsync(DateTime cutoff, int skip, int take);
}
