using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GestorGastos.Api.Dtos.Categories;
using GestorGastos.Domain.Entities;
using GestorGastos.Tests.Integration.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace GestorGastos.Tests.Integration;

public class CategoriesControllerTests(PostgresContainerFixture postgres) : IntegrationTestBase(postgres)
{
    [Fact]
    public async Task GetAll_ReturnsGlobalsAndOwnCategories_GlobalsFirstOrderedByName()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        await client.PostAsJsonAsync("/api/categories", new CategoryUpsertRequest("Mi categoria", "#123456", null));

        var response = await client.GetAsync("/api/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
        categories.Should().NotBeNull();
        categories!.Count.Should().Be(9); // 8 seeded defaults + 1 own
        categories.Where(c => c.IsDefault).Should().HaveCount(8);
        categories.Should().ContainSingle(c => c.Name == "Mi categoria" && !c.IsDefault);
    }

    [Fact]
    public async Task Create_WithValidData_Returns201()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/categories", new CategoryUpsertRequest("Suscripciones", "#4F46E5", "star"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CategoryDto>();
        body!.Name.Should().Be("Suscripciones");
        body.IsDefault.Should().BeFalse();
    }

    [Fact]
    public async Task Create_WithInvalidColor_Returns400()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/categories", new CategoryUpsertRequest("Suscripciones", "not-a-color", null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithDuplicateNameForSameUser_Returns409()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        await client.PostAsJsonAsync("/api/categories", new CategoryUpsertRequest("Repetida", "#4F46E5", null));

        var response = await client.PostAsJsonAsync("/api/categories", new CategoryUpsertRequest("repetida", "#000000", null));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Update_OwnCategory_Returns200()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var created = await (await client.PostAsJsonAsync("/api/categories", new CategoryUpsertRequest("Original", "#4F46E5", null)))
            .Content.ReadFromJsonAsync<CategoryDto>();

        var response = await client.PutAsJsonAsync($"/api/categories/{created!.Id}", new CategoryUpsertRequest("Renombrada", "#059669", null));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CategoryDto>();
        body!.Name.Should().Be("Renombrada");
    }

    [Fact]
    public async Task Update_GlobalCategory_Returns404()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var globalId = await GetDefaultCategoryIdAsync();

        var response = await client.PutAsJsonAsync($"/api/categories/{globalId}", new CategoryUpsertRequest("Hackeada", "#000000", null));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_OtherUsersCategory_Returns404()
    {
        var (ownerClient, _) = await CreateAuthenticatedClientAsync();
        var created = await (await ownerClient.PostAsJsonAsync("/api/categories", new CategoryUpsertRequest("De otro", "#4F46E5", null)))
            .Content.ReadFromJsonAsync<CategoryDto>();

        var (intruderClient, _) = await CreateAuthenticatedClientAsync();

        var response = await intruderClient.PutAsJsonAsync($"/api/categories/{created!.Id}", new CategoryUpsertRequest("Robada", "#000000", null));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_OwnCategoryWithoutExpenses_Returns204()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var created = await (await client.PostAsJsonAsync("/api/categories", new CategoryUpsertRequest("Borrable", "#4F46E5", null)))
            .Content.ReadFromJsonAsync<CategoryDto>();

        var response = await client.DeleteAsync($"/api/categories/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_CategoryWithActiveExpenses_Returns409()
    {
        var (client, user) = await CreateAuthenticatedClientAsync();
        var category = await (await client.PostAsJsonAsync("/api/categories", new CategoryUpsertRequest("Con gastos", "#4F46E5", null)))
            .Content.ReadFromJsonAsync<CategoryDto>();

        await using var db = CreateDbContext();
        db.Expenses.Add(new Expense
        {
            UserId = user.Id,
            CategoryId = category!.Id,
            Amount = 10m,
            SpentAt = DateOnly.FromDateTime(DateTime.UtcNow),
        });
        await db.SaveChangesAsync();

        var response = await client.DeleteAsync($"/api/categories/{category.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private async Task<Guid> GetDefaultCategoryIdAsync()
    {
        await using var db = CreateDbContext();
        return (await db.Categories.FirstAsync(c => c.IsDefault)).Id;
    }
}
