using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace GestorGastos.Api.Csv;

/// <summary>Raw CSV row for expense import; all fields are strings and validated later.</summary>
public class ExpenseCsvRow
{
    public string? Fecha { get; set; }
    public string? Categoria { get; set; }
    public string? Monto { get; set; }
    public string? Nota { get; set; }
}

public record ExpenseCsvExport(string Fecha, string Categoria, string Monto, string? Nota);

public static class ExpenseCsv
{
    private static readonly CsvConfiguration Config = new(CultureInfo.InvariantCulture)
    {
        // Match headers case-insensitively and ignore surrounding whitespace.
        PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant(),
        MissingFieldFound = null,
        HeaderValidated = null,
        TrimOptions = TrimOptions.Trim,
    };

    private static readonly string[] RequiredHeaders = ["fecha", "categoria", "monto"];

    public static IReadOnlyList<ExpenseCsvRow> Read(Stream stream)
    {
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, Config);

        if (!csv.Read() || !csv.ReadHeader())
            throw new CsvFormatException("El archivo está vacío o no tiene cabecera.");

        var headers = (csv.HeaderRecord ?? [])
            .Select(h => h.Trim().ToLowerInvariant())
            .ToHashSet();
        var missing = RequiredHeaders.Where(h => !headers.Contains(h)).ToList();
        if (missing.Count > 0)
            throw new CsvFormatException($"Faltan columnas en el CSV: {string.Join(", ", missing)}.");

        return csv.GetRecords<ExpenseCsvRow>().ToList();
    }

    public static byte[] Write(IEnumerable<ExpenseCsvExport> records)
    {
        using var memory = new MemoryStream();
        // Emit a UTF-8 BOM so Excel on Windows renders accented characters correctly.
        using (var writer = new StreamWriter(memory, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), leaveOpen: true))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteField("fecha");
            csv.WriteField("categoria");
            csv.WriteField("monto");
            csv.WriteField("nota");
            csv.NextRecord();
            foreach (var record in records)
            {
                csv.WriteField(record.Fecha);
                csv.WriteField(record.Categoria);
                csv.WriteField(record.Monto);
                csv.WriteField(record.Nota);
                csv.NextRecord();
            }
        }

        return memory.ToArray();
    }
}

public class CsvFormatException(string message) : Exception(message);
