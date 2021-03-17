using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleSchedulerData
{
    public interface IDatabaseFactory
        : IAsyncDisposable
    {
        Task<IDatabase> GetDatabaseAsync(CancellationToken cancellationToken);
        void MarkForRollback();
    }
}
