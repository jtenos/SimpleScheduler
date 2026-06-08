using Dapper;

namespace SimpleSchedulerData;

/// <summary>
/// Abstraction over the database. Implemented by <see cref="SqlServerDatabase"/> (command text is a
/// stored-procedure name) and <see cref="SqliteDatabase"/> (command text is a SQL script). Managers
/// depend on this interface so the provider can be swapped via configuration.
/// </summary>
public interface IDatabase
{
    Task<T[]> GetManyAsync<T>(string commandText, DynamicParameters? parameters);

    Task<(T1[], T2[])> GetManyAsync<T1, T2>(string commandText, DynamicParameters? parameters);

    Task<T> GetOneAsync<T>(string commandText, DynamicParameters? parameters)
        where T : class;

    Task<T?> GetZeroOrOneAsync<T>(string commandText, DynamicParameters? parameters)
        where T : class;

    Task NonQueryAsync(string commandText, DynamicParameters? parameters);
}
