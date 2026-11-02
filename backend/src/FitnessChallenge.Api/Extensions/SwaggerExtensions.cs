using System.Reflection;
using Microsoft.OpenApi.Models;

namespace FitnessChallenge.Api.Extensions;

public static class SwaggerExtensions
{
    private const string BearerSchemeId = "Bearer";

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Fitness Challenge API",
                Version = "v1",
                Description = "Log activities, normalize them into points and rank users on a global leaderboard.",
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));

            var bearerScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Paste the token returned by POST /api/auth/login.",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = BearerSchemeId,
                },
            };
            options.AddSecurityDefinition(BearerSchemeId, bearerScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement { [bearerScheme] = Array.Empty<string>() });
        });

        return services;
    }
}
