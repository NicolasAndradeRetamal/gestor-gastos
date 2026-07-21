using GestorGastos.Domain.Entities;
using GestorGastos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestorGastos.Infrastructure.Recurring;

public interface IRecurringExpenseGenerator
{
    /// <summary>Creates the expenses due on <paramref name="today"/>; idempotent within a month.</summary>
    Task<int> GenerateDueAsync(DateOnly today, CancellationToken ct);
}

public class RecurringExpenseGenerator(AppDbContext db) : IRecurringExpenseGenerator
{
    public async Task<int> GenerateDueAsync(DateOnly today, CancellationToken ct)
    {
        var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
        var templates = await db.RecurringExpenses.ToListAsync(ct);

        var created = 0;
        foreach (var template in templates)
        {
            var effectiveDay = Math.Min(template.DayOfMonth, daysInMonth);

            // Not due yet this month.
            if (today.Day < effectiveDay)
                continue;

            // Already generated this month.
            if (template.LastRunOn is { } last && last.Year == today.Year && last.Month == today.Month)
                continue;

            db.Expenses.Add(new Expense
            {
                UserId = template.UserId,
                CategoryId = template.CategoryId,
                Amount = template.Amount,
                SpentAt = new DateOnly(today.Year, today.Month, effectiveDay),
                Note = template.Note,
            });
            template.LastRunOn = today;
            created++;
        }

        if (created > 0)
            await db.SaveChangesAsync(ct);

        return created;
    }
}
