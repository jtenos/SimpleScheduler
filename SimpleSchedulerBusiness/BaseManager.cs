using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using SimpleSchedulerData;
using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleSchedulerBusiness
{
    public abstract class BaseManager
    {
        private readonly IDatabaseFactory _databaseFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMemoryCache _cache;
        protected BaseManager(IDatabaseFactory databaseFactory, IServiceProvider serviceProvider, IMemoryCache cache)
            => (_databaseFactory, _serviceProvider, _cache) = (databaseFactory, serviceProvider, cache);

        protected IJobManager GetJobManager() => _serviceProvider.GetRequiredService<IJobManager>();
        protected IScheduleManager GetScheduleManager() => _serviceProvider.GetRequiredService<IScheduleManager>();
        protected IWorkerManager GetWorkerManager() => _serviceProvider.GetRequiredService<IWorkerManager>();

        protected IMemoryCache Cache => _cache;

        protected async Task<int> NonQueryAsync(string sql, DynamicParameters parameters, CancellationToken cancellationToken)
            => await (await _databaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false))
                .NonQueryAsync(sql, parameters, cancellationToken).ConfigureAwait(false);

        protected async Task<T> ScalarAsync<T>(string sql, DynamicParameters parameters, CancellationToken cancellationToken)
            => await (await _databaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false))
                .ScalarAsync<T>(sql, parameters, cancellationToken).ConfigureAwait(false);

        protected async Task<T> GetOneAsync<T>(string sql, DynamicParameters parameters, CancellationToken cancellationToken)
            => await (await _databaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false))
                .GetOneAsync<T>(sql, parameters, cancellationToken).ConfigureAwait(false);

        protected async Task<ImmutableArray<T>> GetManyAsync<T>(string sql, DynamicParameters parameters, CancellationToken cancellationToken)
            => await (await _databaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false))
                .GetManyAsync<T>(sql, parameters, cancellationToken).ConfigureAwait(false);

        protected static DynamicParameters CreateDynamicParameters() => new();
    }
}
// TODO: Move this to data project

