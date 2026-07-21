using FluentValidation;
using GestorGastos.Api.Auth;
using GestorGastos.Api.Dtos.Recurring;
using GestorGastos.Domain.Common;
using GestorGastos.Domain.Entities;
using GestorGastos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestorGastos.Api.Controllers;

[ApiController]
[Route("api/recurring-expenses")]
[Authorize]
public class RecurringExpensesController(
    AppDbContext db,
    IValidator<RecurringExpenseUpsertRequest> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RecurringExpenseDto>>> GetAll(CancellationToken ct)
    {
        var userId = User.GetId();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var templates = await db.RecurringExpenses
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .Join(
                db.Categories,
                r => r.CategoryId,
                c => c.Id,
                (r, c) => new { Recurring = r, c.Name, c.Color })
            .ToListAsync(ct);

        var result = templates
            .Select(t => ToDto(t.Recurring, t.Name, t.Color, today))
            .OrderBy(d => d.NextRunOn)
            .ThenBy(d => d.CategoryName)
            .ToList();

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<RecurringExpenseDto>> Create(RecurringExpenseUpsertRequest request, CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);
        var userId = User.GetId();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var category = await GetAccessibleCategoryAsync(request.CategoryId, userId, ct);

        var template = new RecurringExpense
        {
            UserId = userId,
            CategoryId = request.CategoryId,
            Amount = request.Amount,
            DayOfMonth = request.DayOfMonth,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            // Suppress a retroactive charge if this month's day already passed.
            LastRunOn = today.Day > EffectiveDay(request.DayOfMonth, today) ? today : null,
        };

        db.RecurringExpenses.Add(template);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetAll), null, ToDto(template, category.Name, category.Color, today));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RecurringExpenseDto>> Update(Guid id, RecurringExpenseUpsertRequest request, CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);
        var userId = User.GetId();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var template = await db.RecurringExpenses.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, ct)
            ?? throw new NotFoundException("El gasto recurrente no existe.");

        var category = await GetAccessibleCategoryAsync(request.CategoryId, userId, ct);

        template.CategoryId = request.CategoryId;
        template.Amount = request.Amount;
        template.DayOfMonth = request.DayOfMonth;
        template.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();

        await db.SaveChangesAsync(ct);

        return Ok(ToDto(template, category.Name, category.Color, today));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = User.GetId();

        var template = await db.RecurringExpenses.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, ct)
            ?? throw new NotFoundException("El gasto recurrente no existe.");

        template.Active = false;
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task<Category> GetAccessibleCategoryAsync(Guid categoryId, Guid userId, CancellationToken ct) =>
        await db.Categories.FirstOrDefaultAsync(c => c.Id == categoryId && (c.UserId == null || c.UserId == userId), ct)
            ?? throw new NotFoundException("La categoría no existe.");

    private static RecurringExpenseDto ToDto(RecurringExpense r, string categoryName, string color, DateOnly today) =>
        new(r.Id, r.CategoryId, categoryName, color, r.Amount, r.DayOfMonth, r.Note, NextRun(r.DayOfMonth, r.LastRunOn, today));

    private static int EffectiveDay(int dayOfMonth, DateOnly month) =>
        Math.Min(dayOfMonth, DateTime.DaysInMonth(month.Year, month.Month));

    private static DateOnly NextRun(int dayOfMonth, DateOnly? lastRunOn, DateOnly today)
    {
        var effectiveThisMonth = EffectiveDay(dayOfMonth, today);
        var generatedThisMonth = lastRunOn is { } last && last.Year == today.Year && last.Month == today.Month;

        if (!generatedThisMonth && today.Day <= effectiveThisMonth)
            return new DateOnly(today.Year, today.Month, effectiveThisMonth);

        var nextMonth = today.AddMonths(1);
        return new DateOnly(nextMonth.Year, nextMonth.Month, EffectiveDay(dayOfMonth, nextMonth));
    }
}
