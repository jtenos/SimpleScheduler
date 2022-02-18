using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Polly.Retry;
using System.Collections.Immutable;
using System.Data;

namespace SimpleSchedulerData;

public sealed class SqlDatabase
{
    private readonly string _connectionString;
    private readonly AsyncRetryPolicy _retryPolicy;

    public SqlDatabase(IConfiguration config, AsyncRetryPolicy retryPolicy)
    {
        _connectionString = config.GetConnectionString("SimpleScheduler");
        _retryPolicy = retryPolicy;
    }

    public async Task<ImmutableArray<T>> GetManyAsync<T>(
        string procedureName,
        DynamicParameters? parameters = null,
        int commandTimeoutSeconds = 30,
        CancellationToken cancellationToken = default
    )
    {
        parameters ??= new();

        return await _retryPolicy.ExecuteAsync(async () =>
        {
            CommandDefinition comm = new(
                commandText: procedureName,
                parameters: parameters,
                commandTimeout: commandTimeoutSeconds,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            );

            using SqlConnection conn = await GetOpenConnectionAsync().ConfigureAwait(false);
            return (await conn.QueryAsync<T>(comm).ConfigureAwait(false)).ToImmutableArray();
        }).ConfigureAwait(false);
    }

    public async Task<(ImmutableArray<T1>, ImmutableArray<T2>)> GetManyAsync<T1, T2>(
        string procedureName,
        DynamicParameters? parameters = null,
        int commandTimeoutSeconds = 30,
        CancellationToken cancellationToken = default
    )
    {
        parameters ??= new();

        return await _retryPolicy.ExecuteAsync(async () =>
        {
            CommandDefinition comm = new(
                commandText: procedureName,
                parameters: parameters,
                commandTimeout: commandTimeoutSeconds,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            );

            using SqlConnection conn = await GetOpenConnectionAsync().ConfigureAwait(false);

            using SqlMapper.GridReader multi = await conn.QueryMultipleAsync(comm).ConfigureAwait(false);

            ImmutableArray<T1> result1 = (await multi.ReadAsync<T1>().ConfigureAwait(false)).ToImmutableArray();
            ImmutableArray<T2> result2 = (await multi.ReadAsync<T2>().ConfigureAwait(false)).ToImmutableArray();
            return (result1, result2);
        }).ConfigureAwait(false);
    }

    public async Task<(ImmutableArray<T1>, ImmutableArray<T2>, ImmutableArray<T3>)> GetManyAsync<T1, T2, T3>(
        string procedureName,
        DynamicParameters? parameters = null,
        int commandTimeoutSeconds = 30,
        CancellationToken cancellationToken = default
    )
    {
        parameters ??= new();

        return await _retryPolicy.ExecuteAsync(async () =>
        {
            CommandDefinition comm = new(
                commandText: procedureName,
                parameters: parameters,
                commandTimeout: commandTimeoutSeconds,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            );

            using SqlConnection conn = await GetOpenConnectionAsync().ConfigureAwait(false);

            using SqlMapper.GridReader multi = await conn.QueryMultipleAsync(comm).ConfigureAwait(false);

            ImmutableArray<T1> result1 = (await multi.ReadAsync<T1>().ConfigureAwait(false)).ToImmutableArray();
            ImmutableArray<T2> result2 = (await multi.ReadAsync<T2>().ConfigureAwait(false)).ToImmutableArray();
            ImmutableArray<T3> result3 = (await multi.ReadAsync<T3>().ConfigureAwait(false)).ToImmutableArray();
            return (result1, result2, result3);
        }).ConfigureAwait(false);
    }

    public async Task<(ImmutableArray<T1>, ImmutableArray<T2>, ImmutableArray<T3>, ImmutableArray<T4>)> GetManyAsync<T1, T2, T3, T4>(
        string procedureName,
        DynamicParameters? parameters = null,
        int commandTimeoutSeconds = 30,
        CancellationToken cancellationToken = default
    )
    {
        parameters ??= new();

        return await _retryPolicy.ExecuteAsync(async () =>
        {
            CommandDefinition comm = new(
                commandText: procedureName,
                parameters: parameters,
                commandTimeout: commandTimeoutSeconds,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            );

            using SqlConnection conn = await GetOpenConnectionAsync().ConfigureAwait(false);

            using SqlMapper.GridReader multi = await conn.QueryMultipleAsync(comm).ConfigureAwait(false);

            ImmutableArray<T1> result1 = (await multi.ReadAsync<T1>().ConfigureAwait(false)).ToImmutableArray();
            ImmutableArray<T2> result2 = (await multi.ReadAsync<T2>().ConfigureAwait(false)).ToImmutableArray();
            ImmutableArray<T3> result3 = (await multi.ReadAsync<T3>().ConfigureAwait(false)).ToImmutableArray();
            ImmutableArray<T4> result4 = (await multi.ReadAsync<T4>().ConfigureAwait(false)).ToImmutableArray();
            return (result1, result2, result3, result4);
        }).ConfigureAwait(false);
    }

    public async Task<T> GetOneAsync<T>(
        string procedureName,
        DynamicParameters? parameters = null,
        int commandTimeoutSeconds = 30,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            return (await GetZeroOrOneAsync<T>(procedureName, parameters, commandTimeoutSeconds, cancellationToken).ConfigureAwait(false))!;
        }).ConfigureAwait(false);
    }

    public async Task<T?> GetZeroOrOneAsync<T>(
        string procedureName,
        DynamicParameters? parameters = null,
        int commandTimeoutSeconds = 30,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        parameters ??= new();

        return await _retryPolicy.ExecuteAsync(async () =>
        {
            CommandDefinition comm = new(
                commandText: procedureName,
                parameters: parameters,
                commandTimeout: commandTimeoutSeconds,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken,
                flags: CommandFlags.None
            );

            using SqlConnection conn = await GetOpenConnectionAsync().ConfigureAwait(false);
            foreach (T t in await conn.QueryAsync<T>(comm).ConfigureAwait(false))
            {
                return t;
            }
            return null;
        }).ConfigureAwait(false);
    }

    public async Task NonQueryAsync(
        string procedureName,
        DynamicParameters? parameters = null,
        int commandTimeoutSeconds = 30,
        CancellationToken cancellationToken = default
    )
    {
        parameters ??= new();

        await _retryPolicy.ExecuteAsync(async () =>
        {
            CommandDefinition comm = new(
                commandText: procedureName,
                parameters: parameters,
                commandTimeout: commandTimeoutSeconds,
                cancellationToken: cancellationToken
            );

            using SqlConnection conn = await GetOpenConnectionAsync().ConfigureAwait(false);
            await conn.ExecuteAsync(comm).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    private async Task<SqlConnection> GetOpenConnectionAsync()
    {
        SqlConnection conn = new(_connectionString);
        await conn.OpenAsync();
        return conn;
    }
}
