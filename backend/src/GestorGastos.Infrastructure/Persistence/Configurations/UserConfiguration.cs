using GestorGastos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestorGastos.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasDefaultValueSql("gen_random_uuid()");

        // Email is normalized to lowercase by the application before persisting,
        // so a plain unique index enforces the "unique on lower(email)" rule.
        builder.Property(u => u.Email).HasMaxLength(320).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique().HasDatabaseName("ix_users_email");

        builder.Property(u => u.PasswordHash).HasMaxLength(200).IsRequired();
        builder.Property(u => u.DisplayName).HasMaxLength(100).IsRequired();

        builder.Property(u => u.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(u => u.UpdatedAt).HasDefaultValueSql("now()");
        builder.Property(u => u.Active).HasDefaultValue(true);
    }
}
