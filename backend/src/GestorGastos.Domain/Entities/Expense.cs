using GestorGastos.Domain.Common;

namespace GestorGastos.Domain.Entities;

public class Expense : AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateOnly SpentAt { get; set; }
    public string? Note { get; set; }

    public User? User { get; set; }
    public Category? Category { get; set; }
}
