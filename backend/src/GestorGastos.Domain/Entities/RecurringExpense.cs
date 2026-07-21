using GestorGastos.Domain.Common;

namespace GestorGastos.Domain.Entities;

/// <summary>A template that materializes into a real expense on its day of the month.</summary>
public class RecurringExpense : AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public int DayOfMonth { get; set; }
    public string? Note { get; set; }

    /// <summary>Last date this template generated an expense; guards against duplicates within a month.</summary>
    public DateOnly? LastRunOn { get; set; }

    public Category? Category { get; set; }
}
