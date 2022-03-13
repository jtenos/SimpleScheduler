using SimpleSchedulerModels;

namespace SimpleSchedulerAppServices.Interfaces;

public interface IWorkerManager
{
    Task RunNowAsync(long id);
    Task<Worker[]> GetAllWorkersAsync();
    Task<Worker[]> GetWorkersAsync(long[] ids);
    Task<Worker> GetWorkerAsync(long id);
    Task AddWorkerAsync(string workerName,
        string detailedDescription, string emailOnSuccess, long? parentWorkerID, int timeoutMinutes,
        string directoryName, string executable, string argumentValues, string workerPath);
    Task UpdateWorkerAsync(long id, 
        string workerName, string detailedDescription, string emailOnSuccess, long? parentWorkerID, int timeoutMinutes,
        string directoryName, string executable, string argumentValues, string workerPath);
    Task DeactivateWorkerAsync(long id);
    Task ReactivateWorkerAsync(long id);
}
