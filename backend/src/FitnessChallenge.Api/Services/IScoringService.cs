using FitnessChallenge.Api.Models;

namespace FitnessChallenge.Api.Services;

public interface IScoringService
{
    int CalculatePoints(Sport? sport, decimal? distance, string? duration, int? steps);

    bool TryParseDurationMinutes(string duration, out int wholeMinutes);
}
