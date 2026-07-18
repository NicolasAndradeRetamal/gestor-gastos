using FluentValidation;
using GestorGastos.Api.Auth;
using GestorGastos.Api.Dtos.Common;
using GestorGastos.Api.Dtos.Expenses;
using GestorGastos.Api.Mapping;
using GestorGastos.Domain.Common;
using GestorGastos.Domain.Entities;
using GestorGastos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestorGastos.Api.Controllers;

[ApiController]
[Route("api/expenses")]
[Authorize]
public class ExpensesController(
    AppDbContext db,
    IValidator<ExpenseUpsertRequest> upsertValidator,
    IValidator<ExpenseListQuery> listValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ExpenseDto>>> GetAll([FromQuery] ExpenseListQuery query, CancellationToken ct)
    {
        await listValidator.ValidateAndThrowAsync(query, ct);
        var userId = User.GetId();

        var expenses = db.Expenses.AsNoTracking().Include(e => e.Category).Where(e => e.UserId == userId);

        if (query.From is not null)
            expenses = expenses.Where(e => e.SpentAt >= query.From);
        if (query.To is not null)
            expenses = expenses.Where(e => e.SpentAt <= query.To);
        if (query.CategoryId is not null)
            expenses = expenses.Where(e => e.CategoryId == query.CategoryId);

        expenses = query.Sort switch
        {
            "spentAt" => expenses.OrderBy(e => e.SpentAt).ThenBy(e => e.Id),
            "-spentAt" => expenses.OrderByDescending(e => e.SpentAt).ThenBy(e => e.Id),
            "amount" => expenses.OrderBy(e => e.Amount).ThenBy(e => e.Id),
            "-amount" => expenses.OrderByDescending(e => e.Amount).ThenBy(e => e.Id),
            _ => expenses.OrderByDescending(e => e.SpentAt).ThenBy(e => e.Id),
        };

        var totalItems = await expenses.CountAsync(ct);
        var items = await expenses
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(e => e.ToDto())
            .ToListAsync(ct);

        return Ok(PagedResult<ExpenseDto>.Create(items, query.Page, query.PageSize, totalItems));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExpenseDto>> GetById(Guid id, CancellationToken ct)
    {
        var userId = User.GetId();

        var expense = await db.Expenses.AsNoTracking().Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId, ct)
            ?? throw new NotFoundException("El gasto no existe.");

        return Ok(expense.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Create(ExpenseUpsertRequest request, CancellationToken ct)
    {
        await upsertValidator.ValidateAndThrowAsync(request, ct);
        var userId = User.GetId();

        var category = await GetAccessibleCategory(userId, request.CategoryId, ct);

        var expense = new Expense
        {
            UserId = userId,
            CategoryId = category.Id,
            Amount = request.Amount,
            SpentAt = request.SpentAt,
            Note = request.Note,
        };

        db.Expenses.Add(expense);
        await db.SaveChangesAsync(ct);

        expense.Category = category;
        return CreatedAtAction(nameof(GetById), new { id = expense.Id }, expense.ToDto());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExpenseDto>> Update(Guid id, ExpenseUpsertRequest request, CancellationToken ct)
    {
        await upsertValidator.ValidateAndThrowAsync(request, ct);
        var userId = User.GetId();

        var expense = await db.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId, ct)
            ?? throw new NotFoundException("El gasto no existe.");

        var category = await GetAccessibleCategory(userId, request.CategoryId, ct);

        expense.Amount = request.Amount;
        expense.SpentAt = request.SpentAt;
        expense.Note = request.Note;
        expense.CategoryId = category.Id;

        await db.SaveChangesAsync(ct);

        expense.Category = category;
        return Ok(expense.ToDto());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = User.GetId();

        var expense = await db.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId, ct)
            ?? throw new NotFoundException("El gasto no existe.");

        expense.Active = false;
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task<Category> GetAccessibleCategory(Guid userId, Guid categoryId, CancellationToken ct) =>
        await db.Categories.FirstOrDefaultAsync(c => c.Id == categoryId && (c.UserId == null || c.UserId == userId), ct)
            ?? throw new NotFoundException("La categoría no existe o no es accesible.");
}
