using FitnessChallenge.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FitnessChallenge.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IScoringService, ScoringService>();
        services.AddScoped<IActivityValidator, ActivityValidator>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<ILeaderboardService, LeaderboardService>();

        return services;
    }
}
