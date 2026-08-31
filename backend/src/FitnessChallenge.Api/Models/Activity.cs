namespace FitnessChallenge.Api.Models;

public class Activity
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public DateTime DateTimeUtc { get; set; }

    public Sport? Sport { get; set; }

    public int? Steps { get; set; }
    public decimal? Distance { get; set; }

    public string? Duration { get; set; }

    public int Points { get; set; }
}
