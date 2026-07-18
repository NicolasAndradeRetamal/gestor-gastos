using FluentValidation;
using GestorGastos.Api.Auth;
using GestorGastos.Api.Dtos.Categories;
using GestorGastos.Api.Mapping;
using GestorGastos.Domain.Common;
using GestorGastos.Domain.Entities;
using GestorGastos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestorGastos.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController(AppDbContext db, IValidator<CategoryUpsertRequest> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(CancellationToken ct)
    {
        var userId = User.GetId();

        var categories = await db.Categories
            .AsNoTracking()
            .Where(c => c.UserId == null || c.UserId == userId)
            .OrderByDescending(c => c.UserId == null)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Color, c.Icon, c.IsDefault))
            .ToListAsync(ct);

        return Ok(categories);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CategoryUpsertRequest request, CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);
        var userId = User.GetId();

        await EnsureNameIsAvailable(userId, request.Name, excludingId: null, ct);

        var category = new Category
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Color = request.Color,
            Icon = request.Icon,
            IsDefault = false,
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetAll), null, category.ToDto());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> Update(Guid id, CategoryUpsertRequest request, CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);
        var userId = User.GetId();

        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct)
            ?? throw new NotFoundException("La categoría no existe.");

        await EnsureNameIsAvailable(userId, request.Name, excludingId: id, ct);

        category.Name = request.Name.Trim();
        category.Color = request.Color;
        category.Icon = request.Icon;

        await db.SaveChangesAsync(ct);

        return Ok(category.ToDto());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = User.GetId();

        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct)
            ?? throw new NotFoundException("La categoría no existe.");

        var hasActiveExpenses = await db.Expenses.AnyAsync(e => e.CategoryId == id, ct);
        if (hasActiveExpenses)
            throw new ConflictException("La categoría tiene gastos asociados; reasigna o elimina esos gastos antes de borrarla.");

        category.Active = false;
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task EnsureNameIsAvailable(Guid userId, string name, Guid? excludingId, CancellationToken ct)
    {
        var normalized = name.Trim().ToLowerInvariant();

        var query = db.Categories.Where(c => c.UserId == userId && c.Name.ToLower() == normalized);
        if (excludingId is not null)
            query = query.Where(c => c.Id != excludingId);

        if (await query.AnyAsync(ct))
            throw new ConflictException("Ya tenés una categoría con ese nombre.");
    }
}
