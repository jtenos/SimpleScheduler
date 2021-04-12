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
        protected BaseDatabase(IConfiguration config) => Config = config;
        private DbConnection _connection = default!;
        private DbTransaction _transaction = default!;
        public bool IsInitialized { get; protected set; }
        public void MarkForRollback() => IsMarkedForRollback = true;
        protected IConfiguration Config { get; }
        protected bool IsMarkedForRollback { get; private set; }

        protected abstract DbConnection GetConnection();
        public abstract DbParameter GetInt64Parameter(string name, long? value);
        public DbParameter GetInt64Parameter(string name, bool? value)
            => GetInt64Parameter(name, value == true ? 1 : value == false ? 0 : default(long?));
        public DbParameter GetInt64Parameter(string name, DateTime? value)
            => GetInt64Parameter(name, value.HasValue ? long.Parse(value.Value.ToString("yyyyMMddHHmmssfff")) : default(long?));
        public DbParameter GetInt64Parameter(string name, TimeSpan? value)
            => GetInt64Parameter(name, value.HasValue ? long.Parse(value.Value.ToString("hhmmssfff")) : default(long?));
        public abstract DbParameter GetStringParameter(string name, string? value, bool isFixed, int size);
        public abstract string GetLastAutoIncrementQuery { get; }
        public abstract string GetOffsetLimitClause(int offset, int limit);

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _connection = GetConnection();
            await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            _transaction = await _connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            IsInitialized = true;
        }

        public async Task<ImmutableArray<T>> GetManyAsync<T>(
            string sql, IEnumerable<DbParameter> parameters, Func<DbDataReader, T> mapFunc,
            CancellationToken cancellationToken)
        {
            using DbCommand comm = _connection.CreateCommand();
            comm.Transaction = _transaction;
            comm.CommandText = sql;
            comm.Parameters.AddRange(parameters.ToArray());
            using DbDataReader rdr = await comm.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            var result = new List<T>();
            while (await rdr.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                result.Add(mapFunc(rdr));
            }
            return result.ToImmutableArray();
        }

        public async Task<T> GetOneAsync<T>(
            string sql, IEnumerable<DbParameter> parameters, Func<DbDataReader, T> mapFunc,
            CancellationToken cancellationToken)
            => (await GetManyAsync<T>(
                sql, parameters, mapFunc, cancellationToken).ConfigureAwait(false))[0];

        public async Task<int> NonQueryAsync(
            string sql, IEnumerable<DbParameter> parameters, CancellationToken cancellationToken)
        {
            using var comm = _connection.CreateCommand();
            comm.Transaction = _transaction;
            comm.CommandText = sql;
            comm.Parameters.AddRange(parameters.ToArray());
            return await comm.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task<T> ScalarAsync<T>(
            string sql, IEnumerable<DbParameter> parameters,
            CancellationToken cancellationToken)
        {
            using var comm = _connection.CreateCommand();
            comm.Transaction = _transaction;
            comm.CommandText = sql;
            comm.Parameters.AddRange(parameters.ToArray());
            return (T)(await comm.ExecuteScalarAsync().ConfigureAwait(false))!;
        }

        public async Task CommitAsync(CancellationToken cancellationToken)
            => await _transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        public async ValueTask DisposeAsync()
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
