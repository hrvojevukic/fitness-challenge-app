namespace FitnessChallenge.Application.Dtos;

public class DashboardResponseDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int TotalPoints { get; set; }
    public int ActivityCount { get; set; }
    public List<SportBreakdownDto> SportBreakdown { get; set; } = new();
    public List<DailyVolumeDto> DailyVolume { get; set; } = new();
}

public class SportBreakdownDto
{
    public string Label { get; set; } = string.Empty;
    public int ActivityCount { get; set; }
    public int TotalPoints { get; set; }
}

public class DailyVolumeDto
{
    public DateOnly Date { get; set; }
    public int Points { get; set; }
    public int ActivityCount { get; set; }
}
