using FitnessChallenge.Domain.Entities;

namespace FitnessChallenge.Application.Services;

public class ActivityValidationResult
{
    public List<string> Errors { get; } = new();
    public bool IsValid => Errors.Count == 0;

    public int UserId { get; set; }
    public DateTime DateTimeUtc { get; set; }
    public Sport? Sport { get; set; }
    public int? Steps { get; set; }
    public decimal? Distance { get; set; }
    public string? Duration { get; set; }
}
