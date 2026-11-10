using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolManagement.Infrastructure.Persistence;

namespace SchoolManagement.IntegrationTests.Infrastructure;

internal static class TestDatabaseInitializer
{
    public static async Task<bool> InitializeAsync(SchoolManagementApiFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<SchoolManagementDbContext>();

        var databaseExisted = await dbContext.Database.CanConnectAsync();
        var actualConnectionString = dbContext.Database.GetDbConnection().ConnectionString;

        TestDatabaseSafety.EnsureSafeConnectionString(actualConnectionString);
        await dbContext.Database.MigrateAsync();

        return databaseExisted;
    }
}
