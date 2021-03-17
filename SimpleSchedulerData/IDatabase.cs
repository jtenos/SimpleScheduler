using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace SimpleSchedulerData
{
    public interface IDatabase
        : IAsyncDisposable
    {
        Task InitializeAsync(CancellationToken cancellationToken);
        bool IsInitialized { get; }
        Task<int> NonQueryAsync(string sql, DynamicParameters parameters, CancellationToken cancellationToken);
        Task<T> ScalarAsync<T>(string sql, DynamicParameters parameters, CancellationToken cancellationToken);
        Task<T> GetOneAsync<T>(string sql, DynamicParameters parameters, CancellationToken cancellationToken);
        Task<ImmutableArray<T>> GetManyAsync<T>(string sql, DynamicParameters parameters, CancellationToken cancellationToken);
        Task CommitAsync(CancellationToken cancellationToken);
        void MarkForRollback();
    }
}
