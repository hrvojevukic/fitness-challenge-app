using FitnessChallenge.Domain.Entities;

namespace FitnessChallenge.Application.Services;

public class ScoringService : IScoringService
{
    private static readonly Dictionary<Sport, decimal> PointsPerKm = new()
    {
        [Sport.Running] = 100m,
        [Sport.Walking] = 50m,
        [Sport.Cycling] = 25m,
    };

    private static readonly Dictionary<Sport, int> PointsPerMinute = new()
    {
        [Sport.Swimming] = 15,
        [Sport.Gym] = 5,
    };

    private const int PointsPerHundredSteps = 1;

    public int CalculatePoints(Sport? sport, decimal? distance, string? duration, int? steps)
    {
        if (sport is null)
        {
            var completedHundreds = (steps ?? 0) / 100;
            return completedHundreds * PointsPerHundredSteps;
        }

        if (PointsPerKm.TryGetValue(sport.Value, out var perKm))
        {
            var raw = (distance ?? 0m) * perKm;
            return (int)Math.Floor(raw);
        }

        if (PointsPerMinute.TryGetValue(sport.Value, out var perMinute))
        {
            TryParseDurationMinutes(duration ?? string.Empty, out var wholeMinutes);
            return wholeMinutes * perMinute;
        }

        throw new ArgumentOutOfRangeException(nameof(sport), sport, "Unhandled sport in scoring service.");
    }

    public bool TryParseDurationMinutes(string duration, out int wholeMinutes)
    {
        wholeMinutes = 0;

        if (string.IsNullOrWhiteSpace(duration))
        {
            return false;
        }

        var parts = duration.Split(':');
        if (parts.Length != 2)
        {
            return false;
        }

        if (!int.TryParse(parts[0], out var minutes) || minutes < 0)
        {
            return false;
        }

        if (!int.TryParse(parts[1], out var seconds) || seconds < 0 || seconds > 59)
        {
            return false;
        }

        wholeMinutes = minutes;
        return true;
    }
}
