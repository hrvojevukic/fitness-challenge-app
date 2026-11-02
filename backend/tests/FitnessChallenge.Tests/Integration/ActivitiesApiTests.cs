using System.Net;
using System.Net.Http.Json;
using FitnessChallenge.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace FitnessChallenge.Tests.Integration;

public class ActivitiesApiTests : IClassFixture<TestApiFactory>
{
    private const string Datetime = "2026-06-30T10:30:00Z";

    private readonly TestApiFactory _factory;

    public ActivitiesApiTests(TestApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Logging_without_a_token_is_unauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/activities", new { datetime = Datetime, steps = 500 });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logging_stores_the_activity_with_its_points_under_the_caller()
    {
        var anonymous = _factory.CreateClient();
        var user = await _factory.RegisterAsync(anonymous, "Katherine", "Johnson");
        var client = await _factory.CreateSignedInClientAsync("Katherine", "Johnson");

        var response = await client.PostAsJsonAsync("/api/activities", new { datetime = Datetime, sport = "walking", distance = 1.55 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var activity = await response.Content.ReadFromJsonAsync<ActivityResponse>();
        Assert.Equal(user.Id, activity!.UserId);
        Assert.Equal(77, activity.Points);
    }

    [Fact]
    public async Task Sport_and_metric_mismatch_is_a_bad_request_with_details()
    {
        var anonymous = _factory.CreateClient();
        await _factory.RegisterAsync(anonymous, "Mismatch", "User");
        var client = await _factory.CreateSignedInClientAsync("Mismatch", "User");

        var response = await client.PostAsJsonAsync("/api/activities", new { datetime = Datetime, sport = "swimming", distance = 42.195 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.False(string.IsNullOrWhiteSpace(problem!.Detail));
    }

    [Fact]
    public async Task Logging_for_another_user_is_forbidden()
    {
        var anonymous = _factory.CreateClient();
        var other = await _factory.RegisterAsync(anonymous, "Alan", "Turing");
        await _factory.RegisterAsync(anonymous, "Grace", "Hopper");
        var client = await _factory.CreateSignedInClientAsync("Grace", "Hopper");

        var response = await client.PostAsJsonAsync("/api/activities", new { userId = other.Id.ToString(), datetime = Datetime, steps = 500 });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Only_the_owner_can_delete_an_activity()
    {
        var anonymous = _factory.CreateClient();
        var owner = await _factory.RegisterAsync(anonymous, "Lise", "Meitner");
        await _factory.RegisterAsync(anonymous, "Chien-Shiung", "Wu");
        var ownerClient = await _factory.CreateSignedInClientAsync("Lise", "Meitner");
        var intruderClient = await _factory.CreateSignedInClientAsync("Chien-Shiung", "Wu");
        var created = await ownerClient.PostAsJsonAsync("/api/activities", new { datetime = Datetime, steps = 500 });
        var activity = await created.Content.ReadFromJsonAsync<ActivityResponse>();

        var byIntruder = await intruderClient.DeleteAsync($"/api/activities/{activity!.Id}");
        var byOwner = await ownerClient.DeleteAsync($"/api/activities/{activity.Id}");
        var again = await ownerClient.DeleteAsync($"/api/activities/{activity.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, byIntruder.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, byOwner.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, again.StatusCode);
        var dashboard = await anonymous.GetFromJsonAsync<DashboardResponseDto>($"/api/users/{owner.Id}/dashboard");
        Assert.Equal(0, dashboard!.ActivityCount);
    }

    [Fact]
    public async Task Activity_history_is_paginated_newest_first()
    {
        var anonymous = _factory.CreateClient();
        var user = await _factory.RegisterAsync(anonymous, "Paged", "History");
        var client = await _factory.CreateSignedInClientAsync("Paged", "History");
        foreach (var day in new[] { "01", "02", "03" })
        {
            await client.PostAsJsonAsync("/api/activities", new { datetime = $"2026-06-{day}T10:00:00Z", steps = 500 });
        }

        var first = await anonymous.GetFromJsonAsync<PagedResult<ActivityResponse>>($"/api/activities?userId={user.Id}&page=1&pageSize=2");
        var second = await anonymous.GetFromJsonAsync<PagedResult<ActivityResponse>>($"/api/activities?userId={user.Id}&page=2&pageSize=2");

        Assert.Equal(3, first!.TotalCount);
        Assert.Equal(2, first.TotalPages);
        Assert.Equal(new DateTime(2026, 6, 3, 10, 0, 0, DateTimeKind.Utc), first.Items[0].Datetime);
        Assert.Equal(DateTimeKind.Utc, first.Items[0].Datetime.Kind);
        Assert.Single(second!.Items);
        Assert.Equal(new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc), second.Items[0].Datetime);
    }

    [Fact]
    public async Task Dashboard_aggregates_points_by_sport_and_day()
    {
        var anonymous = _factory.CreateClient();
        var user = await _factory.RegisterAsync(anonymous, "Rosalind", "Franklin");
        var client = await _factory.CreateSignedInClientAsync("Rosalind", "Franklin");
        await client.PostAsJsonAsync("/api/activities", new { datetime = "2026-06-01T08:00:00Z", sport = "running", distance = 3 });
        await client.PostAsJsonAsync("/api/activities", new { datetime = "2026-06-01T18:00:00Z", sport = "running", distance = 1 });
        await client.PostAsJsonAsync("/api/activities", new { datetime = "2026-06-02T08:00:00Z", steps = 500 });

        var dashboard = await anonymous.GetFromJsonAsync<DashboardResponseDto>($"/api/users/{user.Id}/dashboard");
        var mine = await client.GetFromJsonAsync<DashboardResponseDto>("/api/me/dashboard");

        Assert.Equal(405, dashboard!.TotalPoints);
        Assert.Equal(3, dashboard.ActivityCount);

        Assert.Equal("Running", dashboard.SportBreakdown[0].Label);
        Assert.Equal(400, dashboard.SportBreakdown[0].TotalPoints);
        Assert.Equal(2, dashboard.SportBreakdown[0].ActivityCount);
        Assert.Equal("Daily Steps", dashboard.SportBreakdown[1].Label);

        Assert.Equal(2, dashboard.DailyVolume.Count);
        Assert.Equal(new DateOnly(2026, 6, 1), dashboard.DailyVolume[0].Date);
        Assert.Equal(400, dashboard.DailyVolume[0].Points);

        Assert.Equal(user.Id, mine!.UserId);
    }

    [Fact]
    public async Task My_dashboard_requires_a_token()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/me/dashboard");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
