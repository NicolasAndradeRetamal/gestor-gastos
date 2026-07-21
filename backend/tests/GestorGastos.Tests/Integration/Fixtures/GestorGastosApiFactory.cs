using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestorGastos.Tests.Integration.Fixtures;

/// <summary>Boots the real Api host against a caller-supplied Postgres connection string.</summary>
public class GestorGastosApiFactory : WebApplicationFactory<Program>
{
    // Program reads these before builder.Build(), so they must exist as environment
    // variables; ConfigureAppConfiguration is applied too late for those eager reads.
    public GestorGastosApiFactory(string connectionString)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", connectionString);
        Environment.SetEnvironmentVariable("Jwt__Secret", "integration-test-signing-key-please-ignore-32chars+");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "GestorGastosTests");
        Environment.SetEnvironmentVariable("Jwt__Audience", "GestorGastosTests");
        Environment.SetEnvironmentVariable("Jwt__ExpiryMinutes", "60");
        Environment.SetEnvironmentVariable("Totp__EncryptionKey", Convert.ToBase64String(new byte[32]));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }
}
