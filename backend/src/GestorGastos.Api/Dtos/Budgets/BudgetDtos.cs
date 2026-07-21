namespace GestorGastos.Api.Dtos.Budgets;

public record BudgetDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Color,
    decimal Amount,
    decimal Spent,
    int Percentage,
    string Status);

public record BudgetCreateRequest(Guid CategoryId, decimal Amount);

public record BudgetUpdateRequest(decimal Amount);
