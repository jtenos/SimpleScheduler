using Dapper;
using Microsoft.Data.SqlClient;
using Polly.Retry;
using SimpleSchedulerConfiguration.Models;
using System.Data;

namespace SimpleSchedulerData;

public sealed class SqlDatabase
{
    private readonly string _connectionString;
    private readonly AsyncRetryPolicy _retryPolicy;

    public SqlDatabase(AppSettings appSettings, AsyncRetryPolicy retryPolicy)
    {
        _connectionString = appSettings.ConnectionString;
        _retryPolicy = retryPolicy;
    }

    public async Task<T[]> GetManyAsync<T>(
        string procedureName,
        DynamicParameters? parameters
    )
    {
        parameters ??= new();

        return await _retryPolicy.ExecuteAsync(async () =>
        {
            CommandDefinition comm = new(
                commandText: procedureName,
                parameters: parameters,
                commandType: CommandType.StoredProcedure
            );

            using SqlConnection conn = await GetOpenConnectionAsync().ConfigureAwait(false);
            return (await conn.QueryAsync<T>(comm).ConfigureAwait(false)).ToArray();
        }).ConfigureAwait(false);
    }

    //public async Task<(ImmutableArray<T1>, ImmutableArray<T2>)> GetManyAsync<T1, T2>(
    //    string procedureName,
    //    DynamicParameters? parameters,
    //    CancellationToken cancellationToken
    //)
    //{
    //    parameters ??= new();

    //    return await _retryPolicy.ExecuteAsync(async () =>
    //    {
    //        CommandDefinition comm = new(
    //            commandText: procedureName,
    //            parameters: parameters,
    //            commandType: CommandType.StoredProcedure,
    //            cancellationToken: cancellationToken
    //        );

    //        using SqlConnection conn = await GetOpenConnectionAsync().ConfigureAwait(false);

    //        using SqlMapper.GridReader multi = await conn.QueryMultipleAsync(comm).ConfigureAwait(false);

    //        ImmutableArray<T1> result1 = (await multi.ReadAsync<T1>().ConfigureAwait(false)).ToImmutableArray();
    //        ImmutableArray<T2> result2 = (await multi.ReadAsync<T2>().ConfigureAwait(false)).ToImmutableArray();
    //        return (result1, result2);
    //    }).ConfigureAwait(false);
    //}

    //public async Task<(ImmutableArray<T1>, ImmutableArray<T2>, ImmutableArray<T3>)> GetManyAsync<T1, T2, T3>(
    //    string procedureName,
    //    DynamicParameters? parameters,
    //    CancellationToken cancellationToken
    //)
    //{
    //    parameters ??= new();

    //    return await _retryPolicy.ExecuteAsync(async () =>
    //    {
    //        CommandDefinition comm = new(
    //            commandText: procedureName,
    //            parameters: parameters,
    //            commandType: CommandType.StoredProcedure,
    //            cancellationToken: cancellationToken
    //        );

    //        using SqlConnection conn = await GetOpenConnectionAsync().ConfigureAwait(false);

    //        using SqlMapper.GridReader multi = await conn.QueryMultipleAsync(comm).ConfigureAwait(false);

    //        ImmutableArray<T1> result1 = (await multi.ReadAsync<T1>().ConfigureAwait(false)).ToImmutableArray();
    //        ImmutableArray<T2> result2 = (await multi.ReadAsync<T2>().ConfigureAwait(false)).ToImmutableArray();
    //        ImmutableArray<T3> result3 = (await multi.ReadAsync<T3>().ConfigureAwait(false)).ToImmutableArray();
    //        return (result1, result2, result3);
    //    }).ConfigureAwait(false);
    //}

    //public async Task<(ImmutableArray<T1>, ImmutableArray<T2>, ImmutableArray<T3>, ImmutableArray<T4>)> GetManyAsync<T1, T2, T3, T4>(
    //    string procedureName,
    //    DynamicParameters? parameters,
    //    CancellationToken cancellationToken
    //)
    //{
    //    parameters ??= new();

    //    return await _retryPolicy.ExecuteAsync(async () =>
    //    {
    //        CommandDefinition comm = new(
    //            commandText: procedureName,
    //            parameters: parameters,
    //            commandType: CommandType.StoredProcedure,
    //            cancellationToken: cancellationToken
    //        );

    //        using SqlConnection conn = await GetOpenConnectionAsync().ConfigureAwait(false);

    //        using SqlMapper.GridReader multi = await conn.QueryMultipleAsync(comm).ConfigureAwait(false);

    //        ImmutableArray<T1> result1 = (await multi.ReadAsync<T1>().ConfigureAwait(false)).ToImmutableArray();
    //        ImmutableArray<T2> result2 = (await multi.ReadAsync<T2>().ConfigureAwait(false)).ToImmutableArray();
    //        ImmutableArray<T3> result3 = (await multi.ReadAsync<T3>().ConfigureAwait(false)).ToImmutableArray();
    //        ImmutableArray<T4> result4 = (await multi.ReadAsync<T4>().ConfigureAwait(false)).ToImmutableArray();
    //        return (result1, result2, result3, result4);
    //    }).ConfigureAwait(false);
    //}

    public async Task<T> GetOneAsync<T>(
        string procedureName,
        DynamicParameters? parameters
    )
        where T : class
    {
        return (await GetZeroOrOneAsync<T>(procedureName, parameters).ConfigureAwait(false))!;
    }

    public async Task<T?> GetZeroOrOneAsync<T>(
        string procedureName,
        DynamicParameters? parameters
    )
        where T : class
    {
        parameters ??= new();

        return await _retryPolicy.ExecuteAsync(async () =>
        {
            CommandDefinition comm = new(
                commandText: procedureName,
                parameters: parameters,
                commandType: CommandType.StoredProcedure,
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
        DynamicParameters? parameters
    )
    {
        parameters ??= new();

        await _retryPolicy.ExecuteAsync(async () =>
        {
            CommandDefinition comm = new(
                commandText: procedureName,
                parameters: parameters
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
