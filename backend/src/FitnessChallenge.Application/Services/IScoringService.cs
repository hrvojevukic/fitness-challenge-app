using FitnessChallenge.Domain.Entities;

namespace FitnessChallenge.Application.Services;

public interface IScoringService
{
    int CalculatePoints(Sport? sport, decimal? distance, string? duration, int? steps);

    bool TryParseDurationMinutes(string duration, out int wholeMinutes);
}
