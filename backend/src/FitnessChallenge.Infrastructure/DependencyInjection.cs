using FitnessChallenge.Application.Abstractions;
using FitnessChallenge.Application.Services;
using FitnessChallenge.Infrastructure.Auth;
using FitnessChallenge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FitnessChallenge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default") ?? "Data Source=fitness.db";
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<ILeaderboardQuery, LeaderboardQuery>();
        services.AddSingleton<ITokenService, TokenService>();

        return services;
    }

    public static void InitializeDatabase(this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        if (configuration.GetValue("Seed:Enabled", true))
        {
            var scoringService = scope.ServiceProvider.GetRequiredService<IScoringService>();
            DataSeeder.Seed(db, scoringService);
        }
    }
}
