using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using GestorGastos.Api.Dtos.Categories;
using GestorGastos.Api.Dtos.Common;
using GestorGastos.Api.Dtos.Expenses;
using GestorGastos.Api.Dtos.ImportExport;
using GestorGastos.Tests.Integration.Fixtures;

namespace GestorGastos.Tests.Integration;

public class ExpensesImportExportTests(PostgresContainerFixture postgres) : IntegrationTestBase(postgres)
{
    private static readonly DateOnly PastDate = new(2020, 1, 15);

    private static async Task<CategoryDto> FirstCategoryAsync(HttpClient client)
    {
        var categories = await client.GetFromJsonAsync<List<CategoryDto>>("/api/categories");
        return categories![0];
    }

    private static MultipartFormDataContent CsvFile(string csv)
    {
        var form = new MultipartFormDataContent();
        form.Add(new StringContent(csv, Encoding.UTF8, "text/csv"), "file", "test.csv");
        return form;
    }

    [Fact]
    public async Task Preview_ClassifiesValidAndInvalidRows()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var category = await FirstCategoryAsync(client);
        var csv = $"fecha,categoria,monto,nota\n{PastDate:yyyy-MM-dd},{category.Name},42.50,Almuerzo\n2020-02-20,NoExiste,-5,\n";

        var response = await client.PostAsync("/api/expenses/import/preview", CsvFile(csv));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var preview = await response.Content.ReadFromJsonAsync<ImportPreviewDto>();
        preview!.ValidCount.Should().Be(1);
        preview.InvalidCount.Should().Be(1);
        preview.Rows.Should().Contain(r => r.Valid && r.CategoryId != null && r.Amount == 42.50m);
        preview.Rows.Should().Contain(r => !r.Valid && r.Errors.Count >= 2);
    }

    [Fact]
    public async Task Preview_WithMissingColumns_Returns400()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsync("/api/expenses/import/preview", CsvFile("a,b,c\n1,2,3\n"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Import_InsertsOnlyValidRows()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var category = await FirstCategoryAsync(client);
        var request = new ImportConfirmRequest(
        [
            new ImportConfirmRow(PastDate, category.Id, 42.50m, "Almuerzo"),
            new ImportConfirmRow(PastDate, Guid.NewGuid(), 10m, null), // inaccessible category → skipped
            new ImportConfirmRow(PastDate, category.Id, -5m, null),    // non-positive → skipped
        ]);

        var response = await client.PostAsJsonAsync("/api/expenses/import", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ImportResultDto>();
        result!.Imported.Should().Be(1);

        var list = await client.GetFromJsonAsync<PagedResult<ExpenseDto>>("/api/expenses");
        list!.TotalItems.Should().Be(1);
    }

    [Fact]
    public async Task Export_ReturnsCsvOfTheUsersExpenses()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var category = await FirstCategoryAsync(client);
        (await client.PostAsJsonAsync("/api/expenses", new ExpenseUpsertRequest(10.00m, PastDate, "Test", category.Id)))
            .EnsureSuccessStatusCode();

        var response = await client.GetAsync("/api/expenses/export");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
        var text = await response.Content.ReadAsStringAsync();
        text.Should().Contain("fecha,categoria,monto,nota");
        text.Should().Contain("10.00");
        text.Should().Contain(category.Name);
    }

    [Fact]
    public async Task ExportedCsv_CanBeReimported()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var category = await FirstCategoryAsync(client);
        (await client.PostAsJsonAsync("/api/expenses", new ExpenseUpsertRequest(10.00m, PastDate, "Test", category.Id)))
            .EnsureSuccessStatusCode();

        var csv = await (await client.GetAsync("/api/expenses/export")).Content.ReadAsStringAsync();

        var preview = await (await client.PostAsync("/api/expenses/import/preview", CsvFile(csv)))
            .Content.ReadFromJsonAsync<ImportPreviewDto>();

        preview!.ValidCount.Should().Be(1);
        preview.InvalidCount.Should().Be(0);
    }
}
