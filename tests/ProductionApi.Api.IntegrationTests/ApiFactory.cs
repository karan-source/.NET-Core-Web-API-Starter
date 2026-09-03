using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ProductionApi.Api.IntegrationTests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    // A file-backed database so EF Core and Dapper (separate connections) see the same data.
    private readonly string _databasePath =
        Path.Combine(Path.GetTempPath(), $"productionapi-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = $"Data Source={_databasePath}",
                ["Jwt:Issuer"] = "ProductionApi.Tests",
                ["Jwt:Audience"] = "ProductionApi.Tests",
                ["Jwt:SigningKey"] = "integration-test-signing-key-0123456789abcdef",
                ["Jwt:ExpiryMinutes"] = "5"
            }));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }
}
