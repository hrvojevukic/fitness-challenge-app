using FitnessChallenge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessChallenge.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Activity> Activities { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
