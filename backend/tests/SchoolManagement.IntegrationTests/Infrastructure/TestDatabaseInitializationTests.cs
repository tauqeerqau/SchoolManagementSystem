using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolManagement.Infrastructure.Persistence;
using Xunit.Abstractions;

namespace SchoolManagement.IntegrationTests.Infrastructure;

public sealed class TestDatabaseInitializationTests(ITestOutputHelper output)
{
    [Fact]
    public async Task DedicatedTestDatabase_HasInitialMigrationAndExpectedTables()
    {
        using var factory = new SchoolManagementApiFactory();
        var databaseExisted = await TestDatabaseInitializer.InitializeAsync(factory);

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<SchoolManagementDbContext>();

        var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
        const string expectedMigration = "20261106204852_InitialCreate";
        Assert.Contains(expectedMigration, appliedMigrations);

        var connection = dbContext.Database.GetDbConnection();
        var openedConnectionHere = connection.State != ConnectionState.Open;

        if (openedConnectionHere)
        {
            await connection.OpenAsync();
        }

        bool expectedTablesExist;
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT CASE
                    WHEN OBJECT_ID(N'dbo.Students', N'U') IS NOT NULL
                     AND OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NOT NULL
                    THEN 1
                    ELSE 0
                END;
                """;

            expectedTablesExist = Convert.ToInt32(await command.ExecuteScalarAsync()) == 1;
        }
        finally
        {
            if (openedConnectionHere)
            {
                await connection.CloseAsync();
            }
        }

        Assert.True(expectedTablesExist, "Expected Students and __EFMigrationsHistory tables.");
        output.WriteLine(databaseExisted
            ? "SchoolManagementDB_Test already existed; EF Core verified/applied migrations."
            : "SchoolManagementDB_Test did not exist; EF Core created it and applied migrations.");
        output.WriteLine($"Applied migration: {expectedMigration}");
        output.WriteLine("Verified tables: dbo.Students, dbo.__EFMigrationsHistory");
    }
}
