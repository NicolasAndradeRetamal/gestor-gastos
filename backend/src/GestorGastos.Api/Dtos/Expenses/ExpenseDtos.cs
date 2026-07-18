namespace GestorGastos.Api.Dtos.Expenses;

public record ExpenseCategoryDto(Guid Id, string Name, string Color);

public record ExpenseDto(Guid Id, decimal Amount, DateOnly SpentAt, string? Note, ExpenseCategoryDto Category);

public record ExpenseUpsertRequest(decimal Amount, DateOnly SpentAt, string? Note, Guid CategoryId);

/// <summary>Bound from query string parameters of GET /api/expenses.</summary>
public class ExpenseListQuery
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public Guid? CategoryId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string Sort { get; set; } = "-spentAt";
}
