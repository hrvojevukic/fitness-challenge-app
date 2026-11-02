using FitnessChallenge.Application.Abstractions;
using FitnessChallenge.Application.Dtos;
using FitnessChallenge.Application.Exceptions;
using FitnessChallenge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessChallenge.Application.Services;

public interface IUserService
{
    Task<UserResponse> RegisterAsync(RegisterUserRequest request);
    Task<PagedResult<UserResponse>> GetPageAsync(PageQuery paging);
    Task<UserResponse> GetByIdAsync(int id);
}

public class UserService : IUserService
{
    private readonly IAppDbContext _db;

    public UserService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<UserResponse> RegisterAsync(RegisterUserRequest request)
    {
        var firstName = request.FirstName?.Trim();
        var lastName = request.LastName?.Trim();

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            throw new InvalidRequestException("firstName and lastName are both required.");
        }

        var alreadyExists = await _db.Users.AnyAsync(u =>
            u.FirstName.ToLower() == firstName.ToLower() && u.LastName.ToLower() == lastName.ToLower());

        if (alreadyExists)
        {
            throw new ConflictException($"A user named '{firstName} {lastName}' is already registered.");
        }

        var user = new User { FirstName = firstName, LastName = lastName };
        _db.Users.Add(user);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new ConflictException($"A user named '{firstName} {lastName}' is already registered.");
        }

        return user.ToResponse();
    }

    public Task<PagedResult<UserResponse>> GetPageAsync(PageQuery paging)
    {
        return _db.Users
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ThenBy(u => u.Id)
            .ToPagedResultAsync(paging, user => user.ToResponse());
    }

    public async Task<UserResponse> GetByIdAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null)
        {
            throw new NotFoundException($"No user exists with id '{id}'.");
        }

        return user.ToResponse();
    }
}
