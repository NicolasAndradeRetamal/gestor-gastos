namespace GestorGastos.Api.Dtos.Dashboard;

public record CategoryTotalDto(Guid CategoryId, string CategoryName, string Color, decimal Total);

public record MonthTotalDto(string Month, decimal Total);

public record DashboardSummaryDto(decimal Total, IReadOnlyList<CategoryTotalDto> ByCategory, IReadOnlyList<MonthTotalDto> ByMonth);

public class DashboardSummaryQuery
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
}
