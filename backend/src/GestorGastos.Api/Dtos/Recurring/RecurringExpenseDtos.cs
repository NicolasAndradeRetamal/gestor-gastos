namespace GestorGastos.Api.Dtos.Recurring;

public record RecurringExpenseDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Color,
    decimal Amount,
    int DayOfMonth,
    string? Note,
    DateOnly NextRunOn);

public record RecurringExpenseUpsertRequest(Guid CategoryId, decimal Amount, int DayOfMonth, string? Note);
