using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using SimpleSchedulerModels;

namespace SimpleSchedulerBusiness
{
    public interface IWorkerManager
    {
        Task RunNowAsync(int workerID, CancellationToken cancellationToken);
        Task<ImmutableArray<int>> GetChildWorkerIDsByJobAsync(int jobID, CancellationToken cancellationToken);
        Task<ImmutableArray<Worker>> GetAllWorkersAsync(CancellationToken cancellationToken,
            bool getActive = true, bool getInactive = false);
        Task<ImmutableArray<WorkerDetail>> GetAllWorkerDetailsAsync(CancellationToken cancellationToken,
            bool getActive = true, bool getInactive = false);
        Task<Worker> GetWorkerAsync(int workerID, CancellationToken cancellationToken);
        Task<int> AddWorkerAsync(bool isActive, string workerName,
            string? detailedDescription, string? emailOnSuccess, int? parentWorkerID, int timeoutMinutes, int overdueMinutes,
            string directoryName, string executable, string argumentValues, CancellationToken cancellationToken);
        Task UpdateWorkerAsync(int workerID, bool isActive, string workerName,
            string? detailedDescription, string? emailOnSuccess, int? parentWorkerID, int timeoutMinutes, int overdueMinutes,
            string directoryName, string executable, string argumentValues, CancellationToken cancellationToken);
        Task DeactivateWorkerAsync(int workerID, CancellationToken cancellationToken);
        Task ReactivateWorkerAsync(int workerID, CancellationToken cancellationToken);

    }
}
