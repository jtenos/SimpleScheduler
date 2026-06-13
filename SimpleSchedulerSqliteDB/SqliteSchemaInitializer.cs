using System.Data.Common;
using System.Reflection;

namespace SimpleSchedulerSqliteDB;

/// <summary>
/// Creates the SQLite tables and views (embedded as resources in this assembly) if they do
/// not already exist. All DDL uses CREATE ... IF NOT EXISTS, so this is idempotent and safe to
/// run on every startup. The resources are executed in dependency order (foreign keys / view).
/// </summary>
public static class SqliteSchemaInitializer
{
    // Dependency order: Workers <- Schedules <- Jobs, then standalone tables, then the view.
    private static readonly string[] _orderedResourceNames =
    [
        "SimpleSchedulerSqliteDB.Schema.Tables.Workers.sql",
        "SimpleSchedulerSqliteDB.Schema.Tables.Schedules.sql",
        "SimpleSchedulerSqliteDB.Schema.Tables.Jobs.sql",
        "SimpleSchedulerSqliteDB.Schema.Tables.LoginAttempts.sql",
        "SimpleSchedulerSqliteDB.Schema.Tables.Users.sql",
        "SimpleSchedulerSqliteDB.Schema.Views.JobsWithWorkerID.sql",
    ];

    /// <summary>
    /// Runs all schema DDL against an already-open connection, inside a single transaction.
    /// </summary>
    public static async Task EnsureSchemaAsync(DbConnection openConnection)
    {
        Assembly assembly = typeof(SqliteSchemaInitializer).Assembly;

        using DbTransaction transaction = await openConnection.BeginTransactionAsync().ConfigureAwait(false);

        foreach (string resourceName in _orderedResourceNames)
        {
            string ddl = await ReadResourceAsync(assembly, resourceName).ConfigureAwait(false);

            using DbCommand command = openConnection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = ddl;
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        await transaction.CommitAsync().ConfigureAwait(false);
    }

    private static async Task<string> ReadResourceAsync(Assembly assembly, string resourceName)
    {
        using Stream stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded schema resource '{resourceName}' was not found.");
        using StreamReader reader = new(stream);
        return await reader.ReadToEndAsync().ConfigureAwait(false);
    }
}
