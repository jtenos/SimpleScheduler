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
    public abstract class BaseDatabase
    {
        protected bool IsMarkedForRollback { get; private set; }
        public bool IsInitialized { get; protected set; }
        public void MarkForRollback() => IsMarkedForRollback = true;
        public abstract Task InitializeAsync(CancellationToken cancellationToken);
        public abstract Task CommitAsync(CancellationToken cancellationToken);
        public abstract ValueTask DisposeAsync();
    }

    public abstract class BaseDatabase<TConnection, TTransaction, TParam, TDataReader>
        : BaseDatabase
        where TConnection : DbConnection, new()
        where TTransaction : DbTransaction
        where TParam : DbParameter
        where TDataReader : DbDataReader
    {
        private readonly IConfiguration _config;

        protected BaseDatabase(IConfiguration config) => _config = config;
        private TConnection _connection = default!;
        private TTransaction _transaction = default!;

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _connection = new TConnection() { ConnectionString = _config.GetConnectionString("SimpleScheduler") };
            await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            _transaction = (TTransaction)(await _connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false));
            IsInitialized = true;
        }

        public async Task<ImmutableArray<T>> GetManyAsync<T>(
            string sql, IEnumerable<TParam> parameters, Func<TDataReader, T> mapFunc,
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


        public async Task<T> GetOneAsync<T>(
            string sql, IEnumerable<TParam> parameters, Func<TDataReader, T> mapFunc,
            CancellationToken cancellationToken)
            => (await GetManyAsync<T>(
                sql, parameters, mapFunc, cancellationToken).ConfigureAwait(false))[0];

        public async Task<int> NonQueryAsync(
            string sql, IEnumerable<TParam> parameters, CancellationToken cancellationToken)
        {
            using var comm = _connection.CreateCommand();
            comm.CommandText = sql;
            comm.Parameters.AddRange(parameters.ToArray());
            return await comm.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task<T> ScalarAsync<T>(
            string sql, IEnumerable<TParam> parameters,
            CancellationToken cancellationToken)
        {
            using var comm = _connection.CreateCommand();
            comm.CommandText = sql;
            comm.Parameters.AddRange(parameters.ToArray());
            return (T)(await comm.ExecuteScalarAsync().ConfigureAwait(false))!;
        }

        public override async Task CommitAsync(CancellationToken cancellationToken)
            => await _transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        public override async ValueTask DisposeAsync()
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
