using Testcontainers.PostgreSql;

namespace GestorGastos.Tests.Integration.Fixtures;

/// <summary>
/// Starts one real PostgreSQL container for the whole integration test run.
/// Using Testcontainers instead of the EF Core InMemory provider exercises the
/// actual Npgsql behavior the app relies on (uuid defaults, partial/expression
/// unique indexes, check constraints), which InMemory cannot fake.
/// </summary>
public class PostgresContainerFixture : IAsyncLifetime
{
    public PostgreSqlContainer Container { get; } = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("gestor_gastos")
        .WithUsername("gestor_gastos")
        .WithPassword("gestor_gastos")
        .Build();

    public Task InitializeAsync() => Container.StartAsync();

    public Task DisposeAsync() => Container.DisposeAsync().AsTask();
}

[CollectionDefinition(Name)]
public class PostgresCollection : ICollectionFixture<PostgresContainerFixture>
{
    public const string Name = "Postgres";
}
