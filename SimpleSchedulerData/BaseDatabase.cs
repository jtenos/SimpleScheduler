using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SimpleSchedulerData
{
    public abstract class BaseDatabase<TConnection, TTransaction, TParam, TDataReader>
        : IDatabase<TConnection, TTransaction, TParam, TDataReader>
        where TConnection : DbConnection, new()
        where TTransaction : DbTransaction
        where TParam : DbParameter
        where TDataReader : DbDataReader
    {
        private readonly IConfiguration _config;

        protected BaseDatabase(IConfiguration config) => _config = config;
        private TConnection _connection = default!;
        private TTransaction _transaction = default!;

        protected bool MarkForRollback { get; private set; }

        public bool IsInitialized { get; private set; }

        async Task IDatabase<TConnection, TTransaction, TParam, TDataReader>.InitializeAsync(
            CancellationToken cancellationToken)
        {
            _connection = new TConnection() { ConnectionString = _config.GetConnectionString("SimpleScheduler") };
            await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            _transaction = (TTransaction)(await _connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false));
            IsInitialized = true;
        }

        async Task<ImmutableArray<T>> IDatabase<TConnection, TTransaction, TParam, TDataReader>.GetManyAsync<T>(
            string sql,
            IEnumerable<TParam> parameters, Func<TDataReader, T> mapFunc,
            CancellationToken cancellationToken)
        {
            using var comm = _connection.CreateCommand();
            comm.CommandText = sql;
            comm.Parameters.AddRange(parameters.ToArray());
            using var rdr = (TDataReader)await comm.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            var result = new List<T>();
            while (await rdr.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                result.Add(mapFunc(rdr));
            }
            return result.ToImmutableArray();
        }
        async Task<T> IDatabase<TConnection, TTransaction, TParam, TDataReader>.GetOneAsync<T>(
            string sql, IEnumerable<TParam> parameters, Func<TDataReader, T> mapFunc,
            CancellationToken cancellationToken)
            => (await ((IDatabase<TConnection, TTransaction, TParam, TDataReader>)this).GetManyAsync<T>(
                sql, parameters, mapFunc, cancellationToken).ConfigureAwait(false))[0];

        async Task<int> IDatabase<TConnection, TTransaction, TParam, TDataReader>.NonQueryAsync(
            string sql, IEnumerable<TParam> parameters, CancellationToken cancellationToken)
        {
            using var comm = _connection.CreateCommand();
            comm.CommandText = sql;
            comm.Parameters.AddRange(parameters.ToArray());
            return await comm.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        async Task<T> IDatabase<TConnection, TTransaction, TParam, TDataReader>.ScalarAsync<T>(
            string sql, IEnumerable<TParam> parameters,
            CancellationToken cancellationToken)
        {
            using var comm = _connection.CreateCommand();
            comm.CommandText = sql;
            comm.Parameters.AddRange(parameters.ToArray());
            return (T)(await comm.ExecuteScalarAsync().ConfigureAwait(false))!;
        }

        async Task IDatabase<TConnection, TTransaction, TParam, TDataReader>.CommitAsync(
            CancellationToken cancellationToken)
            => await _transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        void IDatabase<TConnection, TTransaction, TParam, TDataReader>.MarkForRollback() => MarkForRollback = true;

        async ValueTask IAsyncDisposable.DisposeAsync()
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
    }
}
