using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using SchoolManagement.Api.Authentication;

namespace SchoolManagement.IntegrationTests.Infrastructure;

public sealed class SchoolManagementApiFactory : WebApplicationFactory<global::Program>
{
    private const string TestConnectionString =
        "Server=LAPTOP-A83LG6TQ;Database=SchoolManagementDB_Test;Trusted_Connection=True;TrustServerCertificate=True";

    public SchoolManagementApiFactory()
    {
        TestDatabaseSafety.EnsureSafeConnectionString(TestConnectionString);
    }

    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        var token = Services.GetRequiredService<IAccessTokenGenerator>().Generate(
            "integration-student",
            "integration.student@example.com",
            "Integration",
            "Student",
            ["Student"]);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.Token);
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:SchoolManagementDatabase"] = TestConnectionString,
                    ["Jwt:SigningKey"] = "integration-test-signing-key-with-at-least-32-bytes"
                });
        });
    }
}
