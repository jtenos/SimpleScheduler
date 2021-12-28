using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using SimpleSchedulerEntities;
using SimpleSchedulerModels;

namespace SimpleSchedulerBusiness
{
    public interface IWorkerManager
    {
        Task RunNowAsync(long workerID, CancellationToken cancellationToken);
        Task<ImmutableArray<long>> GetChildWorkerIDsByJobAsync(long jobID, CancellationToken cancellationToken);
        Task<ImmutableArray<Worker>> GetAllWorkersAsync(CancellationToken cancellationToken,
            bool getActive = true, bool getInactive = false);
        Task<ImmutableArray<WorkerDetail>> GetAllWorkerDetailsAsync(CancellationToken cancellationToken,
            bool getActive = true, bool getInactive = false);
        Task<Worker> GetWorkerAsync(long workerID, CancellationToken cancellationToken);
        Task<long> AddWorkerAsync(bool isActive, string workerName,
            string detailedDescription, string emailOnSuccess, long? parentWorkerID, long timeoutMinutes,
            string directoryName, string executable, string argumentValues, CancellationToken cancellationToken);
        Task UpdateWorkerAsync(long workerID, bool isActive, string workerName,
            string detailedDescription, string emailOnSuccess, long? parentWorkerID, long timeoutMinutes,
            string directoryName, string executable, string argumentValues, CancellationToken cancellationToken);
        Task<string> CheckCanDeactivateWorkerAsync(long workerID, CancellationToken cancellationToken);
        Task DeactivateWorkerAsync(long workerID, CancellationToken cancellationToken);
        Task ReactivateWorkerAsync(long workerID, CancellationToken cancellationToken);
        Worker ConvertToWorker(WorkerEntity entity);
    }
}
