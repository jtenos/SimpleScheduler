using Microsoft.Data.Sqlite;
using Polly.Retry;
using SimpleSchedulerSqliteDB;
using System.Data;
using System.Data.Common;

namespace SimpleSchedulerData;

/// <summary>
/// SQLite implementation of <see cref="IDatabase"/>. Command text is a SQL script (the SQLite
/// equivalent of the SQL Server stored procedures) executed as <see cref="CommandType.Text"/>.
/// On first use it ensures the schema exists (auto-create). Every connection is opened in WAL mode
/// so multiple connections can read/write concurrently.
/// </summary>
public sealed class SqliteDatabase : BaseDatabase
{
    private readonly string _connectionString;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    public SqliteDatabase(string connectionString, AsyncRetryPolicy retryPolicy)
        : base(retryPolicy)
    {
        _connectionString = connectionString;
        SqliteTypeHandlers.Register();
    }

    protected override CommandType CommandType => CommandType.Text;

    protected override async Task<DbConnection> GetOpenConnectionAsync()
    {
        await EnsureInitializedAsync().ConfigureAwait(false);
        return await OpenConnectionAsync().ConfigureAwait(false);
    }

    private async Task<SqliteConnection> OpenConnectionAsync()
    {
        SqliteConnection conn = new(_connectionString);
        await conn.OpenAsync().ConfigureAwait(false);

        // Optimize for multiple concurrent connections (issue #63).
        using (DbCommand pragma = conn.CreateCommand())
        {
            pragma.CommandText =
                "PRAGMA journal_mode=WAL;"
                + "PRAGMA busy_timeout=5000;"
                + "PRAGMA foreign_keys=ON;";
            await pragma.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        return conn;
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized) { return; }

        await _initLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_initialized) { return; }

            using SqliteConnection conn = await OpenConnectionAsync().ConfigureAwait(false);
            await SqliteSchemaInitializer.EnsureSchemaAsync(conn).ConfigureAwait(false);

            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }
}
