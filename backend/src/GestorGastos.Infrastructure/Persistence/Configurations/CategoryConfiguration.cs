using GestorGastos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestorGastos.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(c => c.UserId).IsRequired(false);
        builder.Property(c => c.Name).HasMaxLength(60).IsRequired();
        builder.Property(c => c.Color).HasMaxLength(7).IsFixedLength().IsRequired();
        builder.Property(c => c.Icon).HasMaxLength(40);
        builder.Property(c => c.IsDefault).HasDefaultValue(false);

        builder.Property(c => c.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(c => c.UpdatedAt).HasDefaultValueSql("now()");
        builder.Property(c => c.Active).HasDefaultValue(true);

        builder.HasOne(c => c.User)
            .WithMany(u => u.Categories)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Non-unique lookup index; the real uniqueness rules (case-insensitive
        // name, scoped to active rows) are partial expression indexes added
        // via raw SQL in the initial migration, which the fluent API cannot express.
        builder.HasIndex(c => c.UserId).HasDatabaseName("ix_categories_user_id");
    }
}
