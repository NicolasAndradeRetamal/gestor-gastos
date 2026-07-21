using FluentValidation;
using GestorGastos.Api.Auth;
using GestorGastos.Api.Dtos.Budgets;
using GestorGastos.Domain.Common;
using GestorGastos.Domain.Entities;
using GestorGastos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestorGastos.Api.Controllers;

[ApiController]
[Route("api/budgets")]
[Authorize]
public class BudgetsController(
    AppDbContext db,
    IValidator<BudgetCreateRequest> createValidator,
    IValidator<BudgetUpdateRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BudgetDto>>> GetAll(CancellationToken ct)
    {
        var userId = User.GetId();
        var (monthStart, monthEnd) = CurrentMonth();

        var budgets = await db.Budgets
            .AsNoTracking()
            .Where(b => b.UserId == userId)
            .Join(
                db.Categories,
                b => b.CategoryId,
                c => c.Id,
                (b, c) => new { b.Id, b.CategoryId, c.Name, c.Color, b.Amount })
            .ToListAsync(ct);

        if (budgets.Count == 0)
            return Ok(Array.Empty<BudgetDto>());

        var categoryIds = budgets.Select(b => b.CategoryId).ToList();
        var spentByCategory = await db.Expenses
            .Where(e => e.UserId == userId
                && categoryIds.Contains(e.CategoryId)
                && e.SpentAt >= monthStart
                && e.SpentAt <= monthEnd)
            .GroupBy(e => e.CategoryId)
            .Select(g => new { CategoryId = g.Key, Total = g.Sum(e => e.Amount) })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Total, ct);

        var result = budgets
            .Select(b =>
            {
                var spent = spentByCategory.GetValueOrDefault(b.CategoryId);
                return BuildDto(b.Id, b.CategoryId, b.Name, b.Color, b.Amount, spent);
            })
            .OrderByDescending(d => d.Percentage)
            .ThenBy(d => d.CategoryName)
            .ToList();

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<BudgetDto>> Create(BudgetCreateRequest request, CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(request, ct);
        var userId = User.GetId();

        var category = await db.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && (c.UserId == null || c.UserId == userId), ct)
            ?? throw new NotFoundException("La categoría no existe.");

        var duplicate = await db.Budgets.AnyAsync(b => b.UserId == userId && b.CategoryId == request.CategoryId, ct);
        if (duplicate)
            throw new ConflictException("Ya tienes un presupuesto para esta categoría.");

        var budget = new Budget
        {
            UserId = userId,
            CategoryId = request.CategoryId,
            Amount = request.Amount,
        };

        db.Budgets.Add(budget);
        await db.SaveChangesAsync(ct);

        var dto = await BuildDtoWithSpentAsync(budget, category, userId, ct);
        return CreatedAtAction(nameof(GetAll), null, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BudgetDto>> Update(Guid id, BudgetUpdateRequest request, CancellationToken ct)
    {
        await updateValidator.ValidateAndThrowAsync(request, ct);
        var userId = User.GetId();

        var budget = await db.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, ct)
            ?? throw new NotFoundException("El presupuesto no existe.");

        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == budget.CategoryId, ct)
            ?? throw new NotFoundException("La categoría no existe.");

        budget.Amount = request.Amount;
        await db.SaveChangesAsync(ct);

        var dto = await BuildDtoWithSpentAsync(budget, category, userId, ct);
        return Ok(dto);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = User.GetId();

        var budget = await db.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, ct)
            ?? throw new NotFoundException("El presupuesto no existe.");

        budget.Active = false;
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task<BudgetDto> BuildDtoWithSpentAsync(Budget budget, Category category, Guid userId, CancellationToken ct)
    {
        var (monthStart, monthEnd) = CurrentMonth();
        var spent = await db.Expenses
            .Where(e => e.UserId == userId
                && e.CategoryId == budget.CategoryId
                && e.SpentAt >= monthStart
                && e.SpentAt <= monthEnd)
            .SumAsync(e => (decimal?)e.Amount, ct) ?? 0m;

        return BuildDto(budget.Id, budget.CategoryId, category.Name, category.Color, budget.Amount, spent);
    }

    private static BudgetDto BuildDto(Guid id, Guid categoryId, string name, string color, decimal amount, decimal spent)
    {
        var ratio = amount > 0 ? spent / amount : 0m;
        var percentage = (int)Math.Round(ratio * 100, MidpointRounding.AwayFromZero);
        var status = ratio > 1m ? "exceeded" : ratio >= 0.8m ? "warning" : "ok";

        return new BudgetDto(id, categoryId, name, color, amount, spent, percentage, status);
    }

    private static (DateOnly Start, DateOnly End) CurrentMonth()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var start = new DateOnly(today.Year, today.Month, 1);
        return (start, start.AddMonths(1).AddDays(-1));
    }
}
