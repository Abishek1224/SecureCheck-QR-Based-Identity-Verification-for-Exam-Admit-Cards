using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace SecureCheck.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncDisposable
{
    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"securecheck-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_databasePath}",
                ["InvigilatorAuth:ApiKey"] = "test-invigilator-api-key",
                ["Cors:AllowedOrigins:0"] = "https://localhost:7102",
                ["Cors:AllowedOrigins:1"] = "http://localhost:5102"
            });
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();

        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }
}
