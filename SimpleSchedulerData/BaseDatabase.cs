using Dapper;
using Polly.Retry;
using System.Data;
using System.Data.Common;

namespace SimpleSchedulerData;

/// <summary>
/// Shared Dapper execution logic for the concrete database implementations. Subclasses supply the
/// connection (<see cref="GetOpenConnectionAsync"/>) and the command type (<see cref="CommandType"/>);
/// for SQL Server the command text is a stored-procedure name, for SQLite it is a SQL script.
/// </summary>
public abstract class BaseDatabase : IDatabase
{
    private readonly AsyncRetryPolicy _retryPolicy;

    protected BaseDatabase(AsyncRetryPolicy retryPolicy)
    {
        _retryPolicy = retryPolicy;
    }

    protected abstract CommandType CommandType { get; }

    protected abstract Task<DbConnection> GetOpenConnectionAsync();

    public async Task<T[]> GetManyAsync<T>(
        string commandText,
        DynamicParameters? parameters
    )
    {
        parameters ??= new();

        return await _retryPolicy.ExecuteAsync(async () =>
        {
            CommandDefinition comm = new(
                commandText: commandText,
                parameters: parameters,
                commandType: CommandType
            );

            using DbConnection conn = await GetOpenConnectionAsync().ConfigureAwait(false);
            return (await conn.QueryAsync<T>(comm).ConfigureAwait(false)).ToArray();
        }).ConfigureAwait(false);
    }

    public async Task<(T1[], T2[])> GetManyAsync<T1, T2>(
        string commandText,
        DynamicParameters? parameters
    )
    {
        parameters ??= new();

        return await _retryPolicy.ExecuteAsync(async () =>
        {
            CommandDefinition comm = new(
                commandText: commandText,
                parameters: parameters,
                commandType: CommandType
            );

            using DbConnection conn = await GetOpenConnectionAsync().ConfigureAwait(false);

            using SqlMapper.GridReader multi = await conn.QueryMultipleAsync(comm).ConfigureAwait(false);

            T1[] result1 = (await multi.ReadAsync<T1>().ConfigureAwait(false)).ToArray();
            T2[] result2 = (await multi.ReadAsync<T2>().ConfigureAwait(false)).ToArray();
            return (result1, result2);
        }).ConfigureAwait(false);
    }

    public async Task<T> GetOneAsync<T>(
        string commandText,
        DynamicParameters? parameters
    )
        where T : class
    {
        return (await GetZeroOrOneAsync<T>(commandText, parameters).ConfigureAwait(false))!;
    }

    public async Task<T?> GetZeroOrOneAsync<T>(
        string commandText,
        DynamicParameters? parameters
    )
        where T : class
    {
        parameters ??= new();

        return await _retryPolicy.ExecuteAsync(async () =>
        {
            // Buffered (the default): unbuffered reads break with SQLite multi-statement scripts,
            // and we only need the first row anyway.
            CommandDefinition comm = new(
                commandText: commandText,
                parameters: parameters,
                commandType: CommandType
            );

            using DbConnection conn = await GetOpenConnectionAsync().ConfigureAwait(false);
            foreach (T t in await conn.QueryAsync<T>(comm).ConfigureAwait(false))
            {
                return t;
            }
            return null;
        }).ConfigureAwait(false);
    }

    public async Task NonQueryAsync(
        string commandText,
        DynamicParameters? parameters
    )
    {
        parameters ??= new();

        await _retryPolicy.ExecuteAsync(async () =>
        {
            CommandDefinition comm = new(
                commandText: commandText,
                parameters: parameters,
                commandType: CommandType
            );

            using DbConnection conn = await GetOpenConnectionAsync().ConfigureAwait(false);
            await conn.ExecuteAsync(comm).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }
}
