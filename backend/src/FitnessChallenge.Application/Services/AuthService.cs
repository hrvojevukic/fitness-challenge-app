using FitnessChallenge.Application.Abstractions;
using FitnessChallenge.Application.Dtos;
using FitnessChallenge.Application.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FitnessChallenge.Application.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<UserResponse> GetCurrentUserAsync(int userId);
}

public class AuthService : IAuthService
{
    private readonly IAppDbContext _db;
    private readonly ITokenService _tokenService;

    public AuthService(IAppDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var firstName = request.FirstName?.Trim();
        var lastName = request.LastName?.Trim();

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            throw new InvalidRequestException("firstName and lastName are both required.");
        }

        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.FirstName.ToLower() == firstName.ToLower() && u.LastName.ToLower() == lastName.ToLower());

        if (user is null)
        {
            throw new AuthenticationFailedException($"No registered user named '{firstName} {lastName}'.");
        }

        return new LoginResponse
        {
            Token = _tokenService.CreateToken(user),
            User = user.ToResponse(),
        };
    }

    public async Task<UserResponse> GetCurrentUserAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null)
        {
            throw new AuthenticationFailedException("The user in this session no longer exists.");
        }

        return user.ToResponse();
    }
}
