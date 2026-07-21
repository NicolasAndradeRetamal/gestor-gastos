namespace GestorGastos.Api.Dtos.ImportExport;

public record ImportPreviewRowDto(
    int RowNumber,
    DateOnly? SpentAt,
    string? CategoryName,
    Guid? CategoryId,
    decimal? Amount,
    string? Note,
    bool Valid,
    IReadOnlyList<string> Errors);

public record ImportPreviewDto(IReadOnlyList<ImportPreviewRowDto> Rows, int ValidCount, int InvalidCount);

public record ImportConfirmRow(DateOnly SpentAt, Guid CategoryId, decimal Amount, string? Note);

public record ImportConfirmRequest(IReadOnlyList<ImportConfirmRow> Rows);

public record ImportResultDto(int Imported);
