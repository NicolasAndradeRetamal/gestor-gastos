using GestorGastos.Domain.Common;

namespace GestorGastos.Domain.Entities;

public class Category : AuditableEntity
{
    /// <summary>Null means a global, predefined category shared by every user.</summary>
    public Guid? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public bool IsDefault { get; set; }

    public User? User { get; set; }
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
