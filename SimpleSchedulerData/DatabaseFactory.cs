using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleSchedulerData
{
    public class DatabaseFactory
        : IAsyncDisposable
    {
        private readonly BaseDatabase _database;
        private bool _markForRollback;
        public DatabaseFactory(BaseDatabase database) => _database = database;

        public async Task<BaseDatabase> GetDatabaseAsync(CancellationToken cancellationToken)
        {
            if (!_database.IsInitialized)
            {
                await _database.InitializeAsync(cancellationToken).ConfigureAwait(false);
            }
            return _database;
        }

        public void MarkForRollback() => _markForRollback = true;

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (_database.IsInitialized)
            {
                if (!_markForRollback) await _database.CommitAsync(default).ConfigureAwait(false);
            }
            await _database.DisposeAsync().ConfigureAwait(false);
        }
    }
}
