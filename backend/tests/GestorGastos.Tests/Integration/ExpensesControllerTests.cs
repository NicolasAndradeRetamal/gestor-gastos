using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GestorGastos.Api.Dtos.Categories;
using GestorGastos.Api.Dtos.Common;
using GestorGastos.Api.Dtos.Expenses;
using GestorGastos.Tests.Integration.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace GestorGastos.Tests.Integration;

public class ExpensesControllerTests(PostgresContainerFixture postgres) : IntegrationTestBase(postgres)
{
    [Fact]
    public async Task Create_WithValidData_Returns201WithLocation()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var categoryId = await GetDefaultCategoryIdAsync();

        var response = await client.PostAsJsonAsync("/api/expenses",
            new ExpenseUpsertRequest(42.50m, DateOnly.FromDateTime(DateTime.UtcNow), "Almuerzo", categoryId));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var body = await response.Content.ReadFromJsonAsync<ExpenseDto>();
        body!.Amount.Should().Be(42.50m);
        body.Note.Should().Be("Almuerzo");
    }

    [Fact]
    public async Task Create_WithNegativeAmount_Returns400()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var categoryId = await GetDefaultCategoryIdAsync();

        var response = await client.PostAsJsonAsync("/api/expenses",
            new ExpenseUpsertRequest(-5m, DateOnly.FromDateTime(DateTime.UtcNow), null, categoryId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithFutureDate_Returns400()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var categoryId = await GetDefaultCategoryIdAsync();

        var response = await client.PostAsJsonAsync("/api/expenses",
            new ExpenseUpsertRequest(10m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), null, categoryId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithInaccessibleCategory_Returns404()
    {
        var (ownerClient, ownerUser) = await CreateAuthenticatedClientAsync();
        var ownCategory = await (await ownerClient.PostAsJsonAsync("/api/categories", new CategoryUpsertRequest("Privada", "#4F46E5", null)))
            .Content.ReadFromJsonAsync<CategoryDto>();

        var (intruderClient, _) = await CreateAuthenticatedClientAsync();

        var response = await intruderClient.PostAsJsonAsync("/api/expenses",
            new ExpenseUpsertRequest(10m, DateOnly.FromDateTime(DateTime.UtcNow), null, ownCategory!.Id));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_OwnExpense_Returns200()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var categoryId = await GetDefaultCategoryIdAsync();
        var created = await CreateExpenseAsync(client, categoryId, 15m);

        var response = await client.GetAsync($"/api/expenses/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_OtherUsersExpense_Returns404()
    {
        var (ownerClient, _) = await CreateAuthenticatedClientAsync();
        var categoryId = await GetDefaultCategoryIdAsync();
        var created = await CreateExpenseAsync(ownerClient, categoryId, 15m);

        var (intruderClient, _) = await CreateAuthenticatedClientAsync();

        var response = await intruderClient.GetAsync($"/api/expenses/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_NonExistentExpense_Returns404()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync($"/api/expenses/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_FiltersByDateRangeAndPaginates()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var categoryId = await GetDefaultCategoryIdAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await CreateExpenseAsync(client, categoryId, 10m, today.AddDays(-40));
        await CreateExpenseAsync(client, categoryId, 20m, today.AddDays(-1));
        await CreateExpenseAsync(client, categoryId, 30m, today);

        var response = await client.GetAsync($"/api/expenses?from={today.AddDays(-10):yyyy-MM-dd}&pageSize=1&page=1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResult<ExpenseDto>>();
        body!.TotalItems.Should().Be(2);
        body.Items.Should().HaveCount(1);
        body.PageSize.Should().Be(1);
    }

    [Fact]
    public async Task Update_OwnExpense_Returns200()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var categoryId = await GetDefaultCategoryIdAsync();
        var created = await CreateExpenseAsync(client, categoryId, 15m);

        var response = await client.PutAsJsonAsync($"/api/expenses/{created.Id}",
            new ExpenseUpsertRequest(99.99m, created.SpentAt, "Actualizado", categoryId));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ExpenseDto>();
        body!.Amount.Should().Be(99.99m);
        body.Note.Should().Be("Actualizado");
    }

    [Fact]
    public async Task Delete_OwnExpense_Returns204()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var categoryId = await GetDefaultCategoryIdAsync();
        var created = await CreateExpenseAsync(client, categoryId, 15m);

        var response = await client.DeleteAsync($"/api/expenses/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getAfterDelete = await client.GetAsync($"/api/expenses/{created.Id}");
        getAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_WithoutToken_Returns401()
    {
        var response = await Client.GetAsync("/api/expenses");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<Guid> GetDefaultCategoryIdAsync()
    {
        await using var db = CreateDbContext();
        return (await db.Categories.FirstAsync(c => c.IsDefault)).Id;
    }

    private static async Task<ExpenseDto> CreateExpenseAsync(HttpClient client, Guid categoryId, decimal amount, DateOnly? spentAt = null)
    {
        var request = new ExpenseUpsertRequest(amount, spentAt ?? DateOnly.FromDateTime(DateTime.UtcNow), null, categoryId);
        var response = await client.PostAsJsonAsync("/api/expenses", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ExpenseDto>())!;
    }
}
