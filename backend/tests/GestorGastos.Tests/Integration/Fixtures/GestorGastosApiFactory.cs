using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace GestorGastos.Tests.Integration.Fixtures;

/// <summary>Boots the real Api host against a caller-supplied Postgres connection string.</summary>
public class GestorGastosApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = connectionString,
                ["Jwt:Secret"] = "integration-test-signing-key-please-ignore-32chars+",
                ["Jwt:Issuer"] = "GestorGastosTests",
                ["Jwt:Audience"] = "GestorGastosTests",
                ["Jwt:ExpiryMinutes"] = "60",
            });
        });
    }
}
