using GestorGastos.Api.Dtos.Expenses;
using GestorGastos.Domain.Entities;

namespace GestorGastos.Api.Mapping;

public static class ExpenseMappingExtensions
{
    public static ExpenseDto ToDto(this Expense expense)
    {
        if (expense.Category is null)
            throw new InvalidOperationException("Expense.Category must be loaded before mapping.");

        var category = new ExpenseCategoryDto(expense.Category.Id, expense.Category.Name, expense.Category.Color);
        return new ExpenseDto(expense.Id, expense.Amount, expense.SpentAt, expense.Note, category);
    }
}
