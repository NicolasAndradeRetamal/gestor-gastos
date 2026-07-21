using GestorGastos.Domain.Common;

namespace GestorGastos.Domain.Entities;

/// <summary>A recurring monthly spending limit for a category.</summary>
public class Budget : AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }

    public Category? Category { get; set; }
}
