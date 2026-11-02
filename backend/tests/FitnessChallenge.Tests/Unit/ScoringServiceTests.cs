using FitnessChallenge.Application.Services;
using FitnessChallenge.Domain.Entities;
using Xunit;

namespace FitnessChallenge.Tests.Unit;

public class ScoringServiceTests
{
    private readonly ScoringService _sut = new();

    [Theory]
    [InlineData(Sport.Running, 10, 1000)]
    [InlineData(Sport.Walking, 10, 500)]
    [InlineData(Sport.Cycling, 10, 250)]
    public void Distance_sports_use_the_correct_rate(Sport sport, decimal km, int expectedPoints)
    {
        Assert.Equal(expectedPoints, _sut.CalculatePoints(sport, km, null, null));
    }

    [Fact]
    public void Distance_points_are_floored()
    {
        Assert.Equal(77, _sut.CalculatePoints(Sport.Walking, 1.55m, null, null));
    }

    [Theory]
    [InlineData(Sport.Swimming, "10:00", 150)]
    [InlineData(Sport.Gym, "10:00", 50)]
    [InlineData(Sport.Swimming, "1:55", 15)]
    [InlineData(Sport.Gym, "1:55", 5)]
    public void Duration_sports_count_only_completed_minutes(Sport sport, string duration, int expectedPoints)
    {
        Assert.Equal(expectedPoints, _sut.CalculatePoints(sport, null, duration, null));
    }

    [Theory]
    [InlineData(399, 3)]
    [InlineData(100, 1)]
    [InlineData(42, 0)]
    public void Steps_count_only_completed_hundreds(int steps, int expectedPoints)
    {
        Assert.Equal(expectedPoints, _sut.CalculatePoints(null, null, null, steps));
    }

    [Theory]
    [InlineData("12:34", true, 12)]
    [InlineData("0:05", true, 0)]
    [InlineData("125:00", true, 125)]
    [InlineData("12:60", false, 0)]
    [InlineData("12", false, 0)]
    [InlineData("abc:def", false, 0)]
    [InlineData("", false, 0)]
    public void Duration_parsing_handles_valid_and_invalid_input(string input, bool expectedSuccess, int expectedMinutes)
    {
        var success = _sut.TryParseDurationMinutes(input, out var minutes);

        Assert.Equal(expectedSuccess, success);
        Assert.Equal(expectedMinutes, minutes);
    }
}
