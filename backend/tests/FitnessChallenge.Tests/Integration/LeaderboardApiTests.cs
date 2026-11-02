using System.Net.Http.Json;
using FitnessChallenge.Application.Dtos;
using Xunit;

namespace FitnessChallenge.Tests.Integration;

public class LeaderboardApiTests
{
    private static string DaysAgo(int days)
    {
        return DateTime.UtcNow.AddDays(-days).ToString("O");
    }

    [Fact]
    public async Task Users_are_ranked_by_total_points_across_pages()
    {
        using var factory = new TestApiFactory();
        var anonymous = factory.CreateClient();
        await AddUserWithStepsAsync(factory, anonymous, "Low", 100);
        await AddUserWithStepsAsync(factory, anonymous, "High", 900);
        await AddUserWithStepsAsync(factory, anonymous, "Mid", 500);

        var first = await anonymous.GetFromJsonAsync<PagedResult<LeaderboardEntryDto>>("/api/leaderboard?page=1&pageSize=2");
        var second = await anonymous.GetFromJsonAsync<PagedResult<LeaderboardEntryDto>>("/api/leaderboard?page=2&pageSize=2");

        Assert.Equal(3, first!.TotalCount);
        Assert.Equal(2, first.TotalPages);
        Assert.Equal(new[] { "High User", "Mid User" }, first.Items.Select(e => e.FullName));
        Assert.Equal(new[] { 1, 2 }, first.Items.Select(e => e.Rank));

        var last = Assert.Single(second!.Items);
        Assert.Equal("Low User", last.FullName);
        Assert.Equal(3, last.Rank);
        Assert.Equal(1, last.TotalPoints);
    }

    [Fact]
    public async Task Users_without_activity_are_listed_last_with_zero_points()
    {
        using var factory = new TestApiFactory();
        var anonymous = factory.CreateClient();
        await factory.RegisterAsync(anonymous, "Idle", "User");
        await AddUserWithStepsAsync(factory, anonymous, "Active", 100);

        var page = await anonymous.GetFromJsonAsync<PagedResult<LeaderboardEntryDto>>("/api/leaderboard");

        Assert.Equal("Active User", page!.Items[0].FullName);
        Assert.Equal("Idle User", page.Items[1].FullName);
        Assert.Equal(0, page.Items[1].TotalPoints);
    }

    [Fact]
    public async Task Trend_compares_the_current_rank_with_the_rank_seven_days_ago()
    {
        using var factory = new TestApiFactory();
        var anonymous = factory.CreateClient();
        await factory.RegisterAsync(anonymous, "Climber", "User");
        await factory.RegisterAsync(anonymous, "Faller", "User");
        await factory.RegisterAsync(anonymous, "Newcomer", "User");
        var climber = await factory.CreateSignedInClientAsync("Climber", "User");
        var faller = await factory.CreateSignedInClientAsync("Faller", "User");
        var newcomer = await factory.CreateSignedInClientAsync("Newcomer", "User");

        await faller.PostAsJsonAsync("/api/activities", new { datetime = DaysAgo(10), sport = "running", distance = 2 });
        await climber.PostAsJsonAsync("/api/activities", new { datetime = DaysAgo(10), sport = "running", distance = 1 });
        await climber.PostAsJsonAsync("/api/activities", new { datetime = DaysAgo(1), sport = "running", distance = 5 });
        await newcomer.PostAsJsonAsync("/api/activities", new { datetime = DaysAgo(1), sport = "running", distance = 1 });

        var page = await anonymous.GetFromJsonAsync<PagedResult<LeaderboardEntryDto>>("/api/leaderboard");
        var entries = page!.Items.ToDictionary(e => e.FullName);

        Assert.Equal(1, entries["Climber User"].Rank);
        Assert.Equal(2, entries["Climber User"].PreviousRank);
        Assert.Equal("up", entries["Climber User"].Trend);

        Assert.Equal(2, entries["Faller User"].Rank);
        Assert.Equal(1, entries["Faller User"].PreviousRank);
        Assert.Equal("down", entries["Faller User"].Trend);

        Assert.Null(entries["Newcomer User"].PreviousRank);
        Assert.Equal("new", entries["Newcomer User"].Trend);
    }

    private static async Task AddUserWithStepsAsync(TestApiFactory factory, HttpClient anonymous, string firstName, int steps)
    {
        await factory.RegisterAsync(anonymous, firstName, "User");
        var client = await factory.CreateSignedInClientAsync(firstName, "User");
        var response = await client.PostAsJsonAsync("/api/activities", new { datetime = DaysAgo(1), steps });
        response.EnsureSuccessStatusCode();
    }
}
