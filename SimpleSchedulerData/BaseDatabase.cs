using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace SimpleSchedulerData
{
    public abstract class BaseDatabase
        : IDatabase
    {
        protected bool MarkForRollback { get; private set; }

        public bool IsInitialized { get; private set; }

        async Task IDatabase.InitializeAsync(CancellationToken cancellationToken)
        {
            await InitializeAsync(cancellationToken).ConfigureAwait(false);
            IsInitialized = true;
        }
        protected abstract Task InitializeAsync(CancellationToken cancellationToken);

        async Task<ImmutableArray<T>> IDatabase.GetManyAsync<T>(string sql, DynamicParameters parameters, CancellationToken cancellationToken)
            => await GetManyAsync<T>(sql, parameters, cancellationToken).ConfigureAwait(false);
        protected abstract Task<ImmutableArray<T>> GetManyAsync<T>(string sql, DynamicParameters parameters, CancellationToken cancellationToken);

        async Task<T> IDatabase.GetOneAsync<T>(string sql, DynamicParameters parameters, CancellationToken cancellationToken)
            => await GetOneAsync<T>(sql, parameters, cancellationToken).ConfigureAwait(false);
        protected abstract Task<T> GetOneAsync<T>(string sql, DynamicParameters parameters, CancellationToken cancellationToken);

        async Task<int> IDatabase.NonQueryAsync(string sql, DynamicParameters parameters, CancellationToken cancellationToken)
            => await NonQueryAsync(sql, parameters, cancellationToken).ConfigureAwait(false);
        protected abstract Task<int> NonQueryAsync(string sql, DynamicParameters parameters, CancellationToken cancellationToken);

        async Task<T> IDatabase.ScalarAsync<T>(string sql, DynamicParameters parameters, CancellationToken cancellationToken)
            => await ScalarAsync<T>(sql, parameters, cancellationToken).ConfigureAwait(false);
        protected abstract Task<T> ScalarAsync<T>(string sql, DynamicParameters parameters, CancellationToken cancellationToken);

        async Task IDatabase.CommitAsync(CancellationToken cancellationToken) => await CommitAsync(cancellationToken).ConfigureAwait(false);
        protected abstract Task CommitAsync(CancellationToken cancellationToken);

        void IDatabase.MarkForRollback() => MarkForRollback = true;

        async ValueTask IAsyncDisposable.DisposeAsync() => await DisposeAsync().ConfigureAwait(false);
        protected abstract ValueTask DisposeAsync();
    }
}
