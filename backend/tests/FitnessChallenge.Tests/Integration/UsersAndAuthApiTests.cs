using System.Net;
using System.Net.Http.Json;
using FitnessChallenge.Application.Dtos;
using Xunit;

namespace FitnessChallenge.Tests.Integration;

public class UsersAndAuthApiTests : IClassFixture<TestApiFactory>
{
    private readonly TestApiFactory _factory;

    public UsersAndAuthApiTests(TestApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Registering_a_user_returns_the_new_id()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/users", new { firstName = "Mae", lastName = "Jemison" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.True(user!.Id > 0);
    }

    [Fact]
    public async Task Registering_the_same_name_twice_is_a_conflict_regardless_of_case()
    {
        var client = _factory.CreateClient();
        await _factory.RegisterAsync(client, "Sally", "Ride");

        var response = await client.PostAsJsonAsync("/api/users", new { firstName = "SALLY", lastName = "ride" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Theory]
    [InlineData("", "Last")]
    [InlineData("First", "   ")]
    public async Task Registering_without_a_full_name_is_a_bad_request(string firstName, string lastName)
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/users", new { firstName, lastName });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_user_is_not_found()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/users/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task User_list_is_paginated()
    {
        var client = _factory.CreateClient();
        await _factory.RegisterAsync(client, "Page", "One");
        await _factory.RegisterAsync(client, "Page", "Two");
        await _factory.RegisterAsync(client, "Page", "Three");

        var page = await client.GetFromJsonAsync<PagedResult<UserResponse>>("/api/users?page=1&pageSize=2");

        Assert.Equal(2, page!.Items.Count);
        Assert.True(page.TotalCount >= 3);
        Assert.True(page.TotalPages >= 2);
    }

    [Theory]
    [InlineData("/api/users?page=0")]
    [InlineData("/api/users?pageSize=0")]
    [InlineData("/api/users?pageSize=101")]
    public async Task Out_of_range_paging_is_a_bad_request(string url)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_matches_the_name_case_insensitively_and_identifies_the_user()
    {
        var client = _factory.CreateClient();
        var user = await _factory.RegisterAsync(client, "Ada", "Lovelace");

        var signedIn = await _factory.CreateSignedInClientAsync("ada", "LOVELACE");
        var me = await signedIn.GetFromJsonAsync<UserResponse>("/api/auth/me");

        Assert.Equal(user.Id, me!.Id);
    }

    [Fact]
    public async Task Login_with_an_unknown_name_is_unauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new { firstName = "No", lastName = "Body" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Openapi_document_describes_the_api()
    {
        var client = _factory.CreateClient();

        var document = await client.GetStringAsync("/swagger/v1/swagger.json");

        Assert.Contains("/api/leaderboard", document);
        Assert.Contains("/api/activities", document);
    }
}
