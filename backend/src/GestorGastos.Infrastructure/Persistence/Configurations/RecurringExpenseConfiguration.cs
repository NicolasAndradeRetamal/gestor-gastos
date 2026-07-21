using GestorGastos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestorGastos.Infrastructure.Persistence.Configurations;

public class RecurringExpenseConfiguration : IEntityTypeConfiguration<RecurringExpense>
{
    public void Configure(EntityTypeBuilder<RecurringExpense> builder)
    {
        builder.ToTable("recurring_expenses", tb =>
        {
            tb.HasCheckConstraint("ck_recurring_expenses_amount_positive", "amount > 0");
            tb.HasCheckConstraint("ck_recurring_expenses_day_of_month", "day_of_month between 1 and 31");
        });

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(r => r.UserId).IsRequired();
        builder.Property(r => r.CategoryId).IsRequired();
        builder.Property(r => r.Amount).HasPrecision(12, 2).IsRequired();
        builder.Property(r => r.DayOfMonth).IsRequired();
        builder.Property(r => r.Note).HasMaxLength(500);
        builder.Property(r => r.LastRunOn).HasColumnType("date");

        builder.Property(r => r.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(r => r.UpdatedAt).HasDefaultValueSql("now()");
        builder.Property(r => r.Active).HasDefaultValue(true);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Category)
            .WithMany()
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.UserId).HasDatabaseName("ix_recurring_expenses_user_id");
    }
}
