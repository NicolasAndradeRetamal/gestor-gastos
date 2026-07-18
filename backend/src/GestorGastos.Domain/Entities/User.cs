using GestorGastos.Domain.Common;

namespace GestorGastos.Domain.Entities;

public class User : AuditableEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
