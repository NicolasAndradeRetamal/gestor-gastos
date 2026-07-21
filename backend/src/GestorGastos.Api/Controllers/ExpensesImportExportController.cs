using System.Globalization;
using GestorGastos.Api.Auth;
using GestorGastos.Api.Csv;
using GestorGastos.Api.Dtos.ImportExport;
using GestorGastos.Domain.Entities;
using GestorGastos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestorGastos.Api.Controllers;

[ApiController]
[Route("api/expenses")]
[Authorize]
public class ExpensesImportExportController(AppDbContext db) : ControllerBase
{
    private const int MaxRows = 1000;

    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] Guid? categoryId,
        CancellationToken ct)
    {
        var userId = User.GetId();

        var query = db.Expenses.AsNoTracking().Include(e => e.Category).Where(e => e.UserId == userId);
        if (from is not null)
            query = query.Where(e => e.SpentAt >= from);
        if (to is not null)
            query = query.Where(e => e.SpentAt <= to);
        if (categoryId is not null)
            query = query.Where(e => e.CategoryId == categoryId);

        var expenses = await query.OrderByDescending(e => e.SpentAt).ThenBy(e => e.Id).ToListAsync(ct);

        var records = expenses.Select(e => new ExpenseCsvExport(
            e.SpentAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            e.Category!.Name,
            e.Amount.ToString(CultureInfo.InvariantCulture),
            e.Note));

        return File(ExpenseCsv.Write(records), "text/csv", "gastos.csv");
    }

    [HttpPost("import/preview")]
    public async Task<ActionResult<ImportPreviewDto>> Preview(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            throw new CsvFormatException("Adjunta un archivo CSV.");

        List<ExpenseCsvRow> rows;
        await using (var stream = file.OpenReadStream())
            rows = ExpenseCsv.Read(stream).ToList();

        if (rows.Count > MaxRows)
            throw new CsvFormatException($"El archivo supera el máximo de {MaxRows} filas.");

        var userId = User.GetId();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var byName = await LoadCategoriesByNameAsync(userId, ct);

        var preview = new List<ImportPreviewRowDto>(rows.Count);
        var rowNumber = 1; // header is row 1
        foreach (var row in rows)
        {
            rowNumber++;
            preview.Add(ValidateRow(row, rowNumber, byName, today));
        }

        var validCount = preview.Count(r => r.Valid);
        return Ok(new ImportPreviewDto(preview, validCount, preview.Count - validCount));
    }

    [HttpPost("import")]
    public async Task<ActionResult<ImportResultDto>> Import(ImportConfirmRequest request, CancellationToken ct)
    {
        var userId = User.GetId();
        if (request.Rows.Count == 0)
            return Ok(new ImportResultDto(0));
        if (request.Rows.Count > MaxRows)
            throw new CsvFormatException($"El archivo supera el máximo de {MaxRows} filas.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var categoryIds = request.Rows.Select(r => r.CategoryId).Distinct().ToList();
        var accessible = (await db.Categories
            .Where(c => categoryIds.Contains(c.Id) && (c.UserId == null || c.UserId == userId))
            .Select(c => c.Id)
            .ToListAsync(ct)).ToHashSet();

        var imported = 0;
        foreach (var row in request.Rows)
        {
            // Re-validate: never trust the client's confirmed rows.
            if (!accessible.Contains(row.CategoryId)) continue;
            if (row.Amount <= 0 || decimal.Round(row.Amount, 2) != row.Amount) continue;
            if (row.SpentAt > today) continue;
            if (row.Note is { Length: > 500 }) continue;

            db.Expenses.Add(new Expense
            {
                UserId = userId,
                CategoryId = row.CategoryId,
                Amount = row.Amount,
                SpentAt = row.SpentAt,
                Note = string.IsNullOrWhiteSpace(row.Note) ? null : row.Note.Trim(),
            });
            imported++;
        }

        if (imported > 0)
            await db.SaveChangesAsync(ct);

        return Ok(new ImportResultDto(imported));
    }

    private async Task<Dictionary<string, Guid>> LoadCategoriesByNameAsync(Guid userId, CancellationToken ct)
    {
        var categories = await db.Categories
            .Where(c => c.UserId == null || c.UserId == userId)
            .Select(c => new { c.Id, c.Name, c.UserId })
            .ToListAsync(ct);

        // On a name collision between a global and an own category, the user's own wins.
        return categories
            .GroupBy(c => c.Name.Trim().ToLowerInvariant())
            .ToDictionary(g => g.Key, g => (g.FirstOrDefault(c => c.UserId != null) ?? g.First()).Id);
    }

    private static ImportPreviewRowDto ValidateRow(ExpenseCsvRow row, int rowNumber, Dictionary<string, Guid> byName, DateOnly today)
    {
        var errors = new List<string>();

        DateOnly? spentAt = null;
        if (DateOnly.TryParseExact(row.Fecha?.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            spentAt = date;
            if (date > today)
                errors.Add("La fecha no puede ser posterior a hoy.");
        }
        else
        {
            errors.Add("Fecha inválida (usa AAAA-MM-DD).");
        }

        var categoryName = row.Categoria?.Trim();
        Guid? categoryId = null;
        if (string.IsNullOrEmpty(categoryName))
            errors.Add("La categoría es obligatoria.");
        else if (byName.TryGetValue(categoryName.ToLowerInvariant(), out var id))
            categoryId = id;
        else
            errors.Add("La categoría no existe.");

        decimal? amount = null;
        if (decimal.TryParse(NormalizeDecimal(row.Monto), NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
        {
            amount = value;
            if (value <= 0)
                errors.Add("El monto debe ser mayor que 0.");
            else if (decimal.Round(value, 2) != value)
                errors.Add("El monto admite como máximo 2 decimales.");
        }
        else
        {
            errors.Add("Monto inválido.");
        }

        var note = string.IsNullOrWhiteSpace(row.Nota) ? null : row.Nota.Trim();
        if (note is { Length: > 500 })
            errors.Add("La nota no puede superar los 500 caracteres.");

        var valid = errors.Count == 0;
        return new ImportPreviewRowDto(rowNumber, spentAt, categoryName, valid ? categoryId : null, amount, note, valid, errors);
    }

    private static string? NormalizeDecimal(string? raw)
    {
        var trimmed = raw?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return trimmed;
        // Accept a comma decimal separator when there's no dot already.
        return trimmed.Contains(',') && !trimmed.Contains('.') ? trimmed.Replace(',', '.') : trimmed;
    }
}
