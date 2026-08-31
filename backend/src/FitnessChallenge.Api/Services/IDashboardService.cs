using FitnessChallenge.Api.Dtos;

namespace FitnessChallenge.Api.Services;

public interface IDashboardService
{
    Task<DashboardResponseDto?> BuildForUserAsync(int userId);
}
