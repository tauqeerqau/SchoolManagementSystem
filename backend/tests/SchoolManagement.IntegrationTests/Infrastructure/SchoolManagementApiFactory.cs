using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace SchoolManagement.IntegrationTests.Infrastructure;

public sealed class SchoolManagementApiFactory : WebApplicationFactory<global::Program>
{
    private const string TestConnectionString =
        "Server=LAPTOP-A83LG6TQ;Database=SchoolManagementDB_Test;Trusted_Connection=True;TrustServerCertificate=True";

    public SchoolManagementApiFactory()
    {
        TestDatabaseSafety.EnsureSafeConnectionString(TestConnectionString);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:SchoolManagementDatabase"] = TestConnectionString
                });
        });
    }
}
