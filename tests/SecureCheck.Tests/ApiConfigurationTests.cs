using System.Text.Json;
using Xunit;

namespace SecureCheck.Tests;

public class ApiConfigurationTests
{
    [Fact]
    public void ApiDefaultConnection_IsPortableRelativeSqlitePath()
    {
        var repositoryRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        var appSettingsPath = Path.Combine(
            repositoryRoot,
            "src",
            "SecureCheck.Api",
            "appsettings.json");

        using var json = JsonDocument.Parse(File.ReadAllText(appSettingsPath));

        var connection = json.RootElement
            .GetProperty("ConnectionStrings")
            .GetProperty("DefaultConnection")
            .GetString();

        Assert.False(string.IsNullOrWhiteSpace(connection));
        Assert.DoesNotContain(":\\", connection!, StringComparison.Ordinal);
        Assert.Equal("Data Source=securecheck.db", connection);
    }
}
