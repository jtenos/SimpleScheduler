using OneOf;
using OneOf.Types;
using SimpleSchedulerModels;
using SimpleSchedulerModels.ResultTypes;

namespace SimpleSchedulerAppServices.Interfaces;

public interface IWorkerManager
{
    Task RunNowAsync(long id);
    Task<Worker[]> GetAllWorkersAsync();
    Task<Worker> GetWorkerAsync(long id);
    Task<OneOf<Success, InvalidExecutable, NameAlreadyExists, CircularReference, Exception>> AddWorkerAsync(string workerName,
        string detailedDescription, string emailOnSuccess, long? parentWorkerID, int timeoutMinutes,
        string directoryName, string executable, string argumentValues);
    Task<OneOf<Success, InvalidExecutable, NameAlreadyExists, CircularReference, Exception>> UpdateWorkerAsync(long id, 
        string workerName, string detailedDescription, string emailOnSuccess, long? parentWorkerID, int timeoutMinutes,
        string directoryName, string executable, string argumentValues);
    Task DeactivateWorkerAsync(long id);
    Task ReactivateWorkerAsync(long id);
}
