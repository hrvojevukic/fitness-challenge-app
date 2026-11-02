namespace FitnessChallenge.Application.Dtos;

public class IngestActivityRequest
{
    public string? UserId { get; set; }
    public string? Datetime { get; set; }
    public string? Sport { get; set; }
    public int? Steps { get; set; }
    public decimal? Distance { get; set; }
    public string? Duration { get; set; }
}

public class ActivityResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Datetime { get; set; }
    public string? Sport { get; set; }
    public int? Steps { get; set; }
    public decimal? Distance { get; set; }
    public string? Duration { get; set; }
    public int Points { get; set; }
}
