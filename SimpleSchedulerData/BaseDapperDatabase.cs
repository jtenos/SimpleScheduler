using System.Collections.Immutable;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace SimpleSchedulerData
{
    public abstract class BaseDapperDatabase<TConnection, TCommand, TTransaction>
        : BaseDatabase
        where TConnection : DbConnection, new()
        where TCommand : DbCommand
        where TTransaction : DbTransaction
    {
        private readonly IConfiguration _config;
        private TConnection _connection = default!;
        private TTransaction _transaction = default!;

        protected BaseDapperDatabase(IConfiguration config) => _config = config;

        protected override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _connection = new TConnection() { ConnectionString = _config.GetConnectionString("SimpleScheduler") };
            await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            _transaction = (TTransaction)(await _connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false));
        }

        protected override async Task CommitAsync(CancellationToken cancellationToken)
            => await _transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        protected override async ValueTask DisposeAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync().ConfigureAwait(false);
            }
            if (_connection != null)
            {
                await _connection.DisposeAsync().ConfigureAwait(false);
            }
        }

        protected override async Task<ImmutableArray<T>> GetManyAsync<T>(string sql, DynamicParameters parameters, CancellationToken cancellationToken)
            => (await _connection.QueryAsync<T>(sql, parameters, _transaction).ConfigureAwait(false)).ToImmutableArray();

        protected override async Task<T> GetOneAsync<T>(string sql, DynamicParameters parameters, CancellationToken cancellationToken)
            => (await GetManyAsync<T>(sql, parameters, cancellationToken).ConfigureAwait(false))[0];

        protected override async Task<int> NonQueryAsync(string sql, DynamicParameters parameters, CancellationToken cancellationToken)
            => await _connection.ExecuteAsync(sql, parameters, _transaction).ConfigureAwait(false);

        protected override async Task<T> ScalarAsync<T>(string sql, DynamicParameters parameters, CancellationToken cancellationToken)
            => await _connection.ExecuteScalarAsync<T>(sql, parameters, _transaction).ConfigureAwait(false);
    }
}
