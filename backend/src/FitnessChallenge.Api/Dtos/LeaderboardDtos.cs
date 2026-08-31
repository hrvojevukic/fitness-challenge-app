namespace FitnessChallenge.Api.Dtos;

public class LeaderboardEntryDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int TotalPoints { get; set; }
    public int Rank { get; set; }
    public int? PreviousRank { get; set; }
    public string Trend { get; set; } = "new";
}
