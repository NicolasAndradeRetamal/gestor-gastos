namespace GestorGastos.Domain.Common;

/// <summary>
/// Base class for all persisted entities: carries the audit and soft-delete
/// fields shared by every table (created_at, updated_at, active).
/// </summary>
public abstract class AuditableEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public bool Active { get; set; } = true;
}
