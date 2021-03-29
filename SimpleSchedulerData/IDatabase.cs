using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleSchedulerData
{
    public interface IDatabase<TConnection, TTransaction, TParam, TDataReader>
        : IAsyncDisposable
        where TConnection : DbConnection
        where TTransaction : DbTransaction
        where TParam : DbParameter
        where TDataReader: DbDataReader
    {
        Task InitializeAsync(CancellationToken cancellationToken);
        bool IsInitialized { get; }
        Task<int> NonQueryAsync(string sql, 
            IEnumerable<TParam> parameters, CancellationToken cancellationToken);
        Task<T> ScalarAsync<T>(string sql,
            IEnumerable<TParam> parameters, CancellationToken cancellationToken);
        Task<T> GetOneAsync<T>(string sql, 
            IEnumerable<TParam> parameters, Func<TDataReader, T> mapFunc, 
            CancellationToken cancellationToken);
        Task<ImmutableArray<T>> GetManyAsync<T>(string sql,
            IEnumerable<TParam> parameters, Func<TDataReader, T> mapFunc,
            CancellationToken cancellationToken);
        Task CommitAsync(CancellationToken cancellationToken);
        void MarkForRollback();
    }
}
