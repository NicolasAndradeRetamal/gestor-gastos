using GestorGastos.Api.Auth;
using GestorGastos.Api.Dtos.Dashboard;
using GestorGastos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestorGastos.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController(AppDbContext db) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> Summary([FromQuery] DashboardSummaryQuery query, CancellationToken ct)
    {
        var userId = User.GetId();

        var expenses = db.Expenses.AsNoTracking().Where(e => e.UserId == userId);

        if (query.From is not null)
            expenses = expenses.Where(e => e.SpentAt >= query.From);
        if (query.To is not null)
            expenses = expenses.Where(e => e.SpentAt <= query.To);

        var total = await expenses.SumAsync(e => (decimal?)e.Amount, ct) ?? 0m;

        var byCategoryRaw = await expenses
            .GroupBy(e => new { e.CategoryId, e.Category!.Name, e.Category!.Color })
            .Select(g => new { g.Key.CategoryId, g.Key.Name, g.Key.Color, Total = g.Sum(e => e.Amount) })
            .OrderByDescending(c => c.Total)
            .ToListAsync(ct);

        var byCategory = byCategoryRaw
            .Select(c => new CategoryTotalDto(c.CategoryId, c.Name, c.Color, c.Total))
            .ToList();

        var byMonthRaw = await expenses
            .GroupBy(e => new { e.SpentAt.Year, e.SpentAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(e => e.Amount) })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync(ct);

        var byMonth = byMonthRaw
            .Select(g => new MonthTotalDto($"{g.Year:D4}-{g.Month:D2}", g.Total))
            .ToList();

        return Ok(new DashboardSummaryDto(total, byCategory, byMonth));
    }
}
