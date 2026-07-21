using GestorGastos.Domain.Common;
using GestorGastos.Domain.Entities;
using GestorGastos.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

namespace GestorGastos.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<TwoFactorRecoveryCode> TwoFactorRecoveryCodes => Set<TwoFactorRecoveryCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<User>().HasQueryFilter(u => u.Active);
        modelBuilder.Entity<Category>().HasQueryFilter(c => c.Active);
        modelBuilder.Entity<Expense>().HasQueryFilter(e => e.Active);
        modelBuilder.Entity<Budget>().HasQueryFilter(b => b.Active);
        modelBuilder.Entity<RefreshToken>().HasQueryFilter(t => t.Active);
        modelBuilder.Entity<TwoFactorRecoveryCode>().HasQueryFilter(c => c.Active);

        modelBuilder.Entity<Category>().HasData(CategorySeedData.GetDefaults());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditInfo();
        return base.SaveChanges();
    }

    private void ApplyAuditInfo()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    break;
            }
        }
    }
}
