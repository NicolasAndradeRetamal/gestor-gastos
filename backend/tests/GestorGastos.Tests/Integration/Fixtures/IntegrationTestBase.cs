using System.Net.Http.Headers;
using System.Net.Http.Json;
using GestorGastos.Api.Dtos.Auth;
using GestorGastos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace GestorGastos.Tests.Integration.Fixtures;

/// <summary>Gives every integration test class an isolated database on the shared container.</summary>
[Collection(PostgresCollection.Name)]
public abstract class IntegrationTestBase(PostgresContainerFixture postgres) : IAsyncLifetime
{
    private GestorGastosApiFactory _factory = null!;
    private string _databaseName = null!;
    protected HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _databaseName = $"test_{Guid.NewGuid():N}";

        await using (var connection = new NpgsqlConnection(postgres.Container.GetConnectionString()))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"CREATE DATABASE \"{_databaseName}\"";
            await command.ExecuteNonQueryAsync();
        }

        var builder = new NpgsqlConnectionStringBuilder(postgres.Container.GetConnectionString())
        {
            Database = _databaseName,
        };

        _factory = new GestorGastosApiFactory(builder.ConnectionString);
        Client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await _factory.DisposeAsync();
    }

    /// <summary>Registers a fresh user and returns an authenticated client + the created user.</summary>
    protected async Task<(HttpClient Client, UserDto User)> CreateAuthenticatedClientAsync(string? email = null)
    {
        var request = new RegisterRequest(
            email ?? $"{Guid.NewGuid():N}@example.com",
            "Password123",
            "Test User");

        var response = await Client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        var authedClient = _factory.CreateClient();
        authedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        return (authedClient, auth.User);
    }

    /// <summary>Creates an unauthenticated client against the same host (for adding a bearer token by hand).</summary>
    protected HttpClient CreateClient() => _factory.CreateClient();

    /// <summary>Direct DbContext access for arranging data the public API cannot create (e.g. cross-user fixtures).</summary>
    protected AppDbContext CreateDbContext()
    {
        var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }
}
