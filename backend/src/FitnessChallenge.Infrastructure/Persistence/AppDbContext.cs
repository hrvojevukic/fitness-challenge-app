using FitnessChallenge.Application.Abstractions;
using FitnessChallenge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessChallenge.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Activity> Activities => Set<Activity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => new { u.FirstName, u.LastName }).IsUnique();
        });

        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasOne(a => a.User)
                  .WithMany(u => u.Activities)
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
