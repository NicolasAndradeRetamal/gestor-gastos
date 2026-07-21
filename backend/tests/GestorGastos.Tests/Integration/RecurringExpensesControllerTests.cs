using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GestorGastos.Api.Dtos.Categories;
using GestorGastos.Api.Dtos.Recurring;
using GestorGastos.Tests.Integration.Fixtures;

namespace GestorGastos.Tests.Integration;

public class RecurringExpensesControllerTests(PostgresContainerFixture postgres) : IntegrationTestBase(postgres)
{
    private static async Task<Guid> CategoryIdAsync(HttpClient client)
    {
        var categories = await client.GetFromJsonAsync<List<CategoryDto>>("/api/categories");
        return categories![0].Id;
    }

    [Fact]
    public async Task Create_ReturnsTemplateWithFutureNextRun()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var categoryId = await CategoryIdAsync(client);

        var response = await client.PostAsJsonAsync(
            "/api/recurring-expenses",
            new RecurringExpenseUpsertRequest(categoryId, 9.99m, 15, "Streaming"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<RecurringExpenseDto>();
        body!.Amount.Should().Be(9.99m);
        body.DayOfMonth.Should().Be(15);
        body.Note.Should().Be("Streaming");
        body.NextRunOn.Should().BeOnOrAfter(DateOnly.FromDateTime(DateTime.UtcNow));
    }

    [Fact]
    public async Task Create_WithInvalidDayOfMonth_Returns400()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var categoryId = await CategoryIdAsync(client);

        var response = await client.PostAsJsonAsync(
            "/api/recurring-expenses",
            new RecurringExpenseUpsertRequest(categoryId, 9.99m, 32, null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithUnknownCategory_Returns404()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/recurring-expenses",
            new RecurringExpenseUpsertRequest(Guid.NewGuid(), 9.99m, 15, null));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ChangesTheTemplate()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var categoryId = await CategoryIdAsync(client);
        var created = await (await client.PostAsJsonAsync(
            "/api/recurring-expenses",
            new RecurringExpenseUpsertRequest(categoryId, 9.99m, 15, "Streaming")))
            .Content.ReadFromJsonAsync<RecurringExpenseDto>();

        var response = await client.PutAsJsonAsync(
            $"/api/recurring-expenses/{created!.Id}",
            new RecurringExpenseUpsertRequest(categoryId, 19.99m, 20, "Streaming Plus"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<RecurringExpenseDto>();
        body!.Amount.Should().Be(19.99m);
        body.DayOfMonth.Should().Be(20);
        body.Note.Should().Be("Streaming Plus");
    }

    [Fact]
    public async Task Delete_RemovesTheTemplate()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var categoryId = await CategoryIdAsync(client);
        var created = await (await client.PostAsJsonAsync(
            "/api/recurring-expenses",
            new RecurringExpenseUpsertRequest(categoryId, 9.99m, 15, null)))
            .Content.ReadFromJsonAsync<RecurringExpenseDto>();

        var response = await client.DeleteAsync($"/api/recurring-expenses/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var list = await client.GetFromJsonAsync<List<RecurringExpenseDto>>("/api/recurring-expenses");
        list.Should().BeEmpty();
    }

    [Fact]
    public async Task Templates_AreIsolatedPerUser()
    {
        var (clientA, _) = await CreateAuthenticatedClientAsync();
        var categoryId = await CategoryIdAsync(clientA);
        (await clientA.PostAsJsonAsync(
            "/api/recurring-expenses",
            new RecurringExpenseUpsertRequest(categoryId, 9.99m, 15, null))).EnsureSuccessStatusCode();

        var (clientB, _) = await CreateAuthenticatedClientAsync();
        var list = await clientB.GetFromJsonAsync<List<RecurringExpenseDto>>("/api/recurring-expenses");

        list.Should().BeEmpty();
    }
}
