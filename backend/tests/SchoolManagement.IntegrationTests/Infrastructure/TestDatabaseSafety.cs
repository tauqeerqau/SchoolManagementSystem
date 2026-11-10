using System.Data.Common;

namespace SchoolManagement.IntegrationTests.Infrastructure;

internal static class TestDatabaseSafety
{
    public const string ExpectedDatabaseName = "SchoolManagementDB_Test";

    /// <summary>
    /// Verifies that a database connection string points only to the dedicated test database.
    /// Call this immediately before any test database creation, migration, cleanup, or reset.
    /// </summary>
    public static void EnsureSafeConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "The integration-test database connection string is missing.");
        }

        var parsedConnectionString = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString
        };

        var configuredDatabaseNames = new List<string>();

        if (parsedConnectionString.TryGetValue("Database", out var database))
        {
            configuredDatabaseNames.Add(Convert.ToString(database) ?? string.Empty);
        }

        if (parsedConnectionString.TryGetValue("Initial Catalog", out var initialCatalog))
        {
            configuredDatabaseNames.Add(Convert.ToString(initialCatalog) ?? string.Empty);
        }

        if (configuredDatabaseNames.Count == 0
            || configuredDatabaseNames.Any(databaseName =>
                !string.Equals(
                    databaseName,
                    ExpectedDatabaseName,
                    StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(
                $"Integration tests may access only the '{ExpectedDatabaseName}' database.");
        }
    }
}
