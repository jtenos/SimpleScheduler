using System.Collections.Immutable;
using OneOf;
using OneOf.Types;
using SimpleSchedulerModels;
using SimpleSchedulerModels.ResultTypes;

namespace SimpleSchedulerAppServices.Interfaces;

public interface IWorkerManager
{
    Task RunNowAsync(long id, CancellationToken cancellationToken);
    Task<ImmutableArray<Worker>> GetAllWorkersAsync(CancellationToken cancellationToken);
    Task<Worker> GetWorkerAsync(long id, CancellationToken cancellationToken);
    Task<OneOf<Success, InvalidExecutable, NameAlreadyExists, CircularReference>> AddWorkerAsync(string workerName,
        string detailedDescription, string emailOnSuccess, long? parentWorkerID, int timeoutMinutes,
        string directoryName, string executable, string argumentValues, CancellationToken cancellationToken);
    Task<OneOf<Success, InvalidExecutable, NameAlreadyExists, CircularReference>> UpdateWorkerAsync(long id, 
        string workerName, string detailedDescription, string emailOnSuccess, long? parentWorkerID, int timeoutMinutes,
        string directoryName, string executable, string argumentValues, CancellationToken cancellationToken);
    Task DeactivateWorkerAsync(long id, CancellationToken cancellationToken);
    Task ReactivateWorkerAsync(long id, CancellationToken cancellationToken);
}
