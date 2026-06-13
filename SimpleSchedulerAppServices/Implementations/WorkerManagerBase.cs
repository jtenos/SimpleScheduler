using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerData;
using SimpleSchedulerDataEntities;
using SimpleSchedulerDomainModels;

namespace SimpleSchedulerAppServices.Implementations;

/// <summary>
/// Database-agnostic logic for the worker manager (executable validation, result handling, model
/// mapping). The provider-specific data access lives in the abstract Core methods.
/// </summary>
public abstract class WorkerManagerBase : IWorkerManager
{
    protected IDatabase Db { get; }

    protected WorkerManagerBase(IDatabase db)
    {
        Db = db;
    }

    // A class with settable properties (not a positional record) so Dapper can map SQLite's
    // INTEGER 0/1 result columns to bool (constructor injection doesn't allow that conversion).
    protected sealed class WorkerWriteResult
    {
        public bool Success { get; set; }
        public bool NameAlreadyExists { get; set; }
        public bool CircularReference { get; set; }
    }

    // ---- provider-specific data access ----
    protected abstract Task RunNowCoreAsync(long id);
    protected abstract Task<WorkerEntity[]> SelectAllCoreAsync(
        string? workerName, string? directoryName, string? executable, bool? activeOnly, bool? inactiveOnly);
    protected abstract Task<WorkerEntity[]> SelectManyCoreAsync(long[] ids);
    protected abstract Task<WorkerEntity> SelectCoreAsync(long id);
    protected abstract Task<WorkerWriteResult> InsertWorkerCoreAsync(
        string workerName, string detailedDescription, string emailOnSuccess, long? parentWorkerID,
        int timeoutMinutes, string directoryName, string executable, string argumentValues);
    protected abstract Task<WorkerWriteResult> UpdateWorkerCoreAsync(long id,
        string workerName, string detailedDescription, string emailOnSuccess, long? parentWorkerID,
        int timeoutMinutes, string directoryName, string executable, string argumentValues);
    protected abstract Task DeactivateWorkerCoreAsync(long id);
    protected abstract Task ReactivateWorkerCoreAsync(long id);

    // ---- agnostic orchestration ----
    async Task IWorkerManager.RunNowAsync(long id)
        => await RunNowCoreAsync(id).ConfigureAwait(false);

    async Task<Worker[]> IWorkerManager.GetAllWorkersAsync(string? workerName, string? directoryName,
        string? executable, bool? activeOnly, bool? inactiveOnly)
        => (await SelectAllCoreAsync(workerName, directoryName, executable, activeOnly, inactiveOnly)
            .ConfigureAwait(false))
            .Select(ModelBuilders.GetWorker).ToArray();

    async Task<Worker[]> IWorkerManager.GetWorkersAsync(long[] ids)
        => (await SelectManyCoreAsync(ids).ConfigureAwait(false))
            .Select(ModelBuilders.GetWorker).ToArray();

    async Task<Worker> IWorkerManager.GetWorkerAsync(long id)
        => ModelBuilders.GetWorker(await SelectCoreAsync(id).ConfigureAwait(false));

    async Task IWorkerManager.AddWorkerAsync(string workerName, string detailedDescription,
        string emailOnSuccess, long? parentWorkerID, int timeoutMinutes, string directoryName,
        string executable, string argumentValues, string workerPath)
    {
        if (!IsValidExecutable(directoryName, executable, workerPath))
        {
            throw new ApplicationException("Invalid executable");
        }

        WorkerWriteResult result = await InsertWorkerCoreAsync(workerName, detailedDescription, emailOnSuccess,
            parentWorkerID, timeoutMinutes, directoryName, executable, argumentValues).ConfigureAwait(false);

        ThrowOnWorkerWriteResult(result);
    }

    async Task IWorkerManager.UpdateWorkerAsync(long workerID, string workerName, string detailedDescription,
        string emailOnSuccess, long? parentWorkerID, int timeoutMinutes, string directoryName,
        string executable, string argumentValues, string workerPath)
    {
        if (!IsValidExecutable(directoryName, executable, workerPath))
        {
            throw new ApplicationException("Invalid executable");
        }

        if (workerID == parentWorkerID)
        {
            throw new ApplicationException("Worker cannot be its own parent");
        }

        WorkerWriteResult result = await UpdateWorkerCoreAsync(workerID, workerName, detailedDescription,
            emailOnSuccess, parentWorkerID, timeoutMinutes, directoryName, executable, argumentValues)
            .ConfigureAwait(false);

        ThrowOnWorkerWriteResult(result);
    }

    async Task IWorkerManager.DeactivateWorkerAsync(long id)
        => await DeactivateWorkerCoreAsync(id).ConfigureAwait(false);

    async Task IWorkerManager.ReactivateWorkerAsync(long id)
        => await ReactivateWorkerCoreAsync(id).ConfigureAwait(false);

    private static void ThrowOnWorkerWriteResult(WorkerWriteResult result)
    {
        if (result.NameAlreadyExists) { throw new ApplicationException("Name already exists"); }
        if (result.CircularReference) { throw new ApplicationException("Circular reference"); }
        if (!result.Success) { throw new ApplicationException("Invalid call to AddWorker"); }
    }

    private static bool IsValidExecutable(string directoryName, string executable, string workerPath)
    {
        if (directoryName.Contains('/')
            || executable.Contains('/')
            || directoryName.Contains('\\')
            || executable.Contains('\\'))
        {
            return false;
        }

        string fullPath = Path.Combine(workerPath, directoryName, executable);
        if (!File.Exists(fullPath))
        {
            return false;
        }

        return true;
    }
}
