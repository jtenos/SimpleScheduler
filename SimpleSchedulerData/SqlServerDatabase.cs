using Microsoft.Data.SqlClient;
using Polly.Retry;
using System.Data;
using System.Data.Common;

namespace SimpleSchedulerData;

/// <summary>
/// SQL Server implementation of <see cref="IDatabase"/>. Command text is a stored-procedure name
/// (e.g. <c>[app].[Jobs_Insert]</c>) executed as <see cref="CommandType.StoredProcedure"/>.
/// </summary>
public sealed class SqlServerDatabase : BaseDatabase
{
    private readonly string _connectionString;

    public SqlServerDatabase(string connectionString, AsyncRetryPolicy retryPolicy)
        : base(retryPolicy)
    {
        _connectionString = connectionString;
    }

    protected override CommandType CommandType => CommandType.StoredProcedure;

    protected override async Task<DbConnection> GetOpenConnectionAsync()
    {
        SqlConnection conn = new(_connectionString);
        await conn.OpenAsync().ConfigureAwait(false);
        return conn;
    }
}
