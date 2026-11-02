using System.Net.Http.Headers;
using System.Net.Http.Json;
using FitnessChallenge.Application.Dtos;
using FitnessChallenge.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitnessChallenge.Tests.Integration;

public class TestApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    static TestApiFactory()
    {
        Environment.SetEnvironmentVariable("Jwt__Key", "test-signing-key-0123456789-0123456789-abcd");
        Environment.SetEnvironmentVariable("Seed__Enabled", "false");
    }

    public async Task<UserResponse> RegisterAsync(HttpClient client, string firstName, string lastName)
    {
        var response = await client.PostAsJsonAsync("/api/users", new { firstName, lastName });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UserResponse>())!;
    }

    public async Task<HttpClient> CreateSignedInClientAsync(string firstName, string lastName)
    {
        var client = CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { firstName, lastName });
        login.EnsureSuccessStatusCode();

        var body = await login.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.Token);
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            var descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            services.Remove(descriptor);
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
