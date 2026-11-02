using FitnessChallenge.Application.Dtos;

namespace FitnessChallenge.Application.Services;

public interface IDashboardService
{
    Task<DashboardResponseDto> BuildForUserAsync(int userId);
}
