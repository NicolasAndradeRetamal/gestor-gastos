using GestorGastos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestorGastos.Infrastructure.Persistence.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("expenses", tb => tb.HasCheckConstraint("ck_expenses_amount_positive", "amount > 0"));

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.CategoryId).IsRequired();
        builder.Property(e => e.Amount).HasPrecision(12, 2).IsRequired();
        builder.Property(e => e.SpentAt).HasColumnType("date").IsRequired();
        builder.Property(e => e.Note).HasMaxLength(500);

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        builder.Property(e => e.Active).HasDefaultValue(true);

        builder.HasOne(e => e.User)
            .WithMany(u => u.Expenses)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Category)
            .WithMany(c => c.Expenses)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.UserId, e.SpentAt }).HasDatabaseName("ix_expenses_user_id_spent_at");
        builder.HasIndex(e => new { e.UserId, e.CategoryId }).HasDatabaseName("ix_expenses_user_id_category_id");
    }
}
