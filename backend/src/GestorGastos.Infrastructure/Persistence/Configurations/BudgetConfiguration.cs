using GestorGastos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestorGastos.Infrastructure.Persistence.Configurations;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("budgets", tb => tb.HasCheckConstraint("ck_budgets_amount_positive", "amount > 0"));

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(b => b.UserId).IsRequired();
        builder.Property(b => b.CategoryId).IsRequired();
        builder.Property(b => b.Amount).HasPrecision(12, 2).IsRequired();

        builder.Property(b => b.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(b => b.UpdatedAt).HasDefaultValueSql("now()");
        builder.Property(b => b.Active).HasDefaultValue(true);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Category)
            .WithMany()
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // One active budget per category per user.
        builder.HasIndex(b => new { b.UserId, b.CategoryId })
            .IsUnique()
            .HasFilter("active = true")
            .HasDatabaseName("ix_budgets_user_id_category_id");
    }
}
