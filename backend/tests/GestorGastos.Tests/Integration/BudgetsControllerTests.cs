using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GestorGastos.Api.Dtos.Budgets;
using GestorGastos.Api.Dtos.Categories;
using GestorGastos.Api.Dtos.Expenses;
using GestorGastos.Tests.Integration.Fixtures;

namespace GestorGastos.Tests.Integration;

public class BudgetsControllerTests(PostgresContainerFixture postgres) : IntegrationTestBase(postgres)
{
    private static async Task<List<Guid>> CategoryIdsAsync(HttpClient client)
    {
        var categories = await client.GetFromJsonAsync<List<CategoryDto>>("/api/categories");
        return categories!.Select(c => c.Id).ToList();
    }

    private static async Task CreateExpenseAsync(HttpClient client, Guid categoryId, decimal amount)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var response = await client.PostAsJsonAsync("/api/expenses", new ExpenseUpsertRequest(amount, today, null, categoryId));
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Create_ReturnsBudgetWithZeroSpentAndOkStatus()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var categoryId = (await CategoryIdsAsync(client))[0];

        var response = await client.PostAsJsonAsync("/api/budgets", new BudgetCreateRequest(categoryId, 500m));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<BudgetDto>();
        body!.Amount.Should().Be(500m);
        body.Spent.Should().Be(0m);
        body.Percentage.Should().Be(0);
        body.Status.Should().Be("ok");
    }

    [Fact]
    public async Task GetAll_ComputesCurrentMonthSpendAndWarningStatus()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var categoryId = (await CategoryIdsAsync(client))[0];
        await client.PostAsJsonAsync("/api/budgets", new BudgetCreateRequest(categoryId, 100m));
        await CreateExpenseAsync(client, categoryId, 85m);

        var budgets = await client.GetFromJsonAsync<List<BudgetDto>>("/api/budgets");

        budgets.Should().ContainSingle();
        budgets![0].Spent.Should().Be(85m);
        budgets[0].Percentage.Should().Be(85);
        budgets[0].Status.Should().Be("warning");
    }

    [Fact]
    public async Task GetAll_MarksBudgetExceededWhenSpendPassesLimit()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var categoryId = (await CategoryIdsAsync(client))[0];
        await client.PostAsJsonAsync("/api/budgets", new BudgetCreateRequest(categoryId, 100m));
        await CreateExpenseAsync(client, categoryId, 130m);

        var budgets = await client.GetFromJsonAsync<List<BudgetDto>>("/api/budgets");

        budgets![0].Percentage.Should().Be(130);
        budgets[0].Status.Should().Be("exceeded");
    }

    [Fact]
    public async Task Create_DuplicateCategory_Returns409()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var categoryId = (await CategoryIdsAsync(client))[0];
        (await client.PostAsJsonAsync("/api/budgets", new BudgetCreateRequest(categoryId, 100m))).EnsureSuccessStatusCode();

        var response = await client.PostAsJsonAsync("/api/budgets", new BudgetCreateRequest(categoryId, 200m));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_UnknownCategory_Returns404()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/budgets", new BudgetCreateRequest(Guid.NewGuid(), 100m));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithNonPositiveAmount_Returns400()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var categoryId = (await CategoryIdsAsync(client))[0];

        var response = await client.PostAsJsonAsync("/api/budgets", new BudgetCreateRequest(categoryId, 0m));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_ChangesLimitAndRecomputesStatus()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var categoryId = (await CategoryIdsAsync(client))[0];
        var created = await (await client.PostAsJsonAsync("/api/budgets", new BudgetCreateRequest(categoryId, 100m)))
            .Content.ReadFromJsonAsync<BudgetDto>();
        await CreateExpenseAsync(client, categoryId, 150m);

        var response = await client.PutAsJsonAsync($"/api/budgets/{created!.Id}", new BudgetUpdateRequest(300m));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<BudgetDto>();
        body!.Amount.Should().Be(300m);
        body.Spent.Should().Be(150m);
        body.Percentage.Should().Be(50);
        body.Status.Should().Be("ok");
    }

    [Fact]
    public async Task Delete_RemovesBudget()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var categoryId = (await CategoryIdsAsync(client))[0];
        var created = await (await client.PostAsJsonAsync("/api/budgets", new BudgetCreateRequest(categoryId, 100m)))
            .Content.ReadFromJsonAsync<BudgetDto>();

        var response = await client.DeleteAsync($"/api/budgets/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var budgets = await client.GetFromJsonAsync<List<BudgetDto>>("/api/budgets");
        budgets.Should().BeEmpty();
    }

    [Fact]
    public async Task Budgets_AreIsolatedPerUser()
    {
        var (clientA, _) = await CreateAuthenticatedClientAsync();
        var categoryId = (await CategoryIdsAsync(clientA))[0];
        (await clientA.PostAsJsonAsync("/api/budgets", new BudgetCreateRequest(categoryId, 100m))).EnsureSuccessStatusCode();

        var (clientB, _) = await CreateAuthenticatedClientAsync();
        var budgets = await clientB.GetFromJsonAsync<List<BudgetDto>>("/api/budgets");

        budgets.Should().BeEmpty();
    }
}
