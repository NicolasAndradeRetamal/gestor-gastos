using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GestorGastos.Api.Dtos.Dashboard;
using GestorGastos.Api.Dtos.Expenses;
using GestorGastos.Tests.Integration.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace GestorGastos.Tests.Integration;

public class DashboardControllerTests(PostgresContainerFixture postgres) : IntegrationTestBase(postgres)
{
    [Fact]
    public async Task Summary_WithNoExpenses_ReturnsZeroTotal()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/dashboard/summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<DashboardSummaryDto>();
        body!.Total.Should().Be(0m);
        body.ByCategory.Should().BeEmpty();
        body.ByMonth.Should().BeEmpty();
    }

    [Fact]
    public async Task Summary_AggregatesTotalsByCategoryAndMonth()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        await using var db = CreateDbContext();
        var comida = await db.Categories.FirstAsync(c => c.Name == "Comida");
        var transporte = await db.Categories.FirstAsync(c => c.Name == "Transporte");

        await client.PostAsJsonAsync("/api/expenses", new ExpenseUpsertRequest(100m, new DateOnly(2026, 6, 15), null, comida.Id));
        await client.PostAsJsonAsync("/api/expenses", new ExpenseUpsertRequest(50m, new DateOnly(2026, 6, 20), null, transporte.Id));
        await client.PostAsJsonAsync("/api/expenses", new ExpenseUpsertRequest(25m, new DateOnly(2026, 7, 1), null, comida.Id));

        var response = await client.GetAsync("/api/dashboard/summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<DashboardSummaryDto>();
        body!.Total.Should().Be(175m);

        body.ByCategory.Should().HaveCount(2);
        body.ByCategory[0].CategoryName.Should().Be("Comida");
        body.ByCategory[0].Total.Should().Be(125m);
        body.ByCategory[1].CategoryName.Should().Be("Transporte");
        body.ByCategory[1].Total.Should().Be(50m);

        body.ByMonth.Should().HaveCount(2);
        body.ByMonth[0].Month.Should().Be("2026-06");
        body.ByMonth[0].Total.Should().Be(150m);
        body.ByMonth[1].Month.Should().Be("2026-07");
        body.ByMonth[1].Total.Should().Be(25m);
    }

    [Fact]
    public async Task Summary_FiltersByDateRange()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        await using var db = CreateDbContext();
        var comida = await db.Categories.FirstAsync(c => c.Name == "Comida");

        await client.PostAsJsonAsync("/api/expenses", new ExpenseUpsertRequest(100m, new DateOnly(2026, 1, 15), null, comida.Id));
        await client.PostAsJsonAsync("/api/expenses", new ExpenseUpsertRequest(50m, new DateOnly(2026, 7, 1), null, comida.Id));

        var response = await client.GetAsync("/api/dashboard/summary?from=2026-06-01&to=2026-07-31");

        var body = await response.Content.ReadFromJsonAsync<DashboardSummaryDto>();
        body!.Total.Should().Be(50m);
    }

    [Fact]
    public async Task Summary_WithoutToken_Returns401()
    {
        var response = await Client.GetAsync("/api/dashboard/summary");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
