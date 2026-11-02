using FitnessChallenge.Application.Dtos;
using FitnessChallenge.Application.Services;
using FitnessChallenge.Domain.Entities;
using Xunit;

namespace FitnessChallenge.Tests.Unit;

public class ActivityValidatorTests
{
    private const string ValidDatetime = "2026-06-30T10:30:00Z";

    private readonly ActivityValidator _sut = new(new ScoringService());

    [Fact]
    public void Running_with_a_distance_is_valid()
    {
        var result = _sut.Validate(new IngestActivityRequest
        {
            UserId = "1",
            Datetime = ValidDatetime,
            Sport = "running",
            Distance = 42.195m,
        });

        Assert.True(result.IsValid);
        Assert.Equal(Sport.Running, result.Sport);
        Assert.Equal(42.195m, result.Distance);
    }

    [Fact]
    public void Swimming_with_a_duration_is_valid()
    {
        var result = _sut.Validate(new IngestActivityRequest
        {
            UserId = "1",
            Datetime = ValidDatetime,
            Sport = "swimming",
            Duration = "12:34",
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Steps_without_a_sport_are_valid()
    {
        var result = _sut.Validate(new IngestActivityRequest
        {
            UserId = "1",
            Datetime = ValidDatetime,
            Steps = 399,
        });

        Assert.True(result.IsValid);
        Assert.Null(result.Sport);
    }

    [Fact]
    public void Swimming_with_a_distance_is_rejected()
    {
        var result = _sut.Validate(new IngestActivityRequest
        {
            UserId = "1",
            Datetime = ValidDatetime,
            Sport = "swimming",
            Distance = 42.195m,
        });

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(null, ValidDatetime, null, 100)]
    [InlineData("abc", ValidDatetime, null, 100)]
    [InlineData("1", null, null, 100)]
    [InlineData("1", "not-a-date", null, 100)]
    [InlineData("1", ValidDatetime, "rowing", null)]
    [InlineData("1", ValidDatetime, null, -1)]
    [InlineData("1", ValidDatetime, null, null)]
    public void Invalid_fields_are_rejected(string? userId, string? datetime, string? sport, int? steps)
    {
        var result = _sut.Validate(new IngestActivityRequest
        {
            UserId = userId,
            Datetime = datetime,
            Sport = sport,
            Steps = steps,
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Malformed_duration_is_rejected()
    {
        var result = _sut.Validate(new IngestActivityRequest
        {
            UserId = "1",
            Datetime = ValidDatetime,
            Sport = "gym",
            Duration = "not-a-duration",
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Steps_alongside_a_distance_sport_are_rejected()
    {
        var result = _sut.Validate(new IngestActivityRequest
        {
            UserId = "1",
            Datetime = ValidDatetime,
            Sport = "running",
            Distance = 5,
            Steps = 100,
        });

        Assert.False(result.IsValid);
    }
}
