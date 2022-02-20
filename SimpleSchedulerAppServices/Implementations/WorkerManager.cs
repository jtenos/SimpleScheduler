using System.Collections.Immutable;
using System.Data;
using Dapper;
using OneOf;
using OneOf.Types;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerConfiguration.Models;
using SimpleSchedulerData;
using SimpleSchedulerModels;
using SimpleSchedulerModels.ResultTypes;

namespace SimpleSchedulerAppServices.Implementations;

public sealed class WorkerManager
    : IWorkerManager
{
    private readonly SqlDatabase _db;
    private readonly AppSettings _appSettings;

    public WorkerManager(SqlDatabase db, AppSettings appSettings)
    {
        _db = db;
        _appSettings = appSettings;
    }

    async Task IWorkerManager.RunNowAsync(long id, CancellationToken cancellationToken)
    {
        DynamicParameters param = new DynamicParameters()
            .AddBigIntArrayParam("@WorkerIDs", new[] { id }.ToImmutableArray());

        await _db.NonQueryAsync("[app].[Jobs_RunNow]",
            param,
            cancellationToken
        ).ConfigureAwait(false);
    }

    async Task<ImmutableArray<Worker>> IWorkerManager.GetAllWorkersAsync(CancellationToken cancellationToken)
    {
        return await _db.GetManyAsync<Worker>(
            "[app].[Workers_SelectAll]",
            parameters: null,
            cancellationToken
        ).ConfigureAwait(false);
    }

    async Task<Worker> IWorkerManager.GetWorkerAsync(long id, CancellationToken cancellationToken)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", id);

        return await _db.GetOneAsync<Worker>(
            "[app].[Workers_Select]",
            param,
            cancellationToken
        ).ConfigureAwait(false);
    }

    private record class AddWorkerResult(bool Success, bool NameAlreadyExists, bool CircularReference);
    async Task<OneOf<Success, InvalidExecutable, NameAlreadyExists, CircularReference>> IWorkerManager.AddWorkerAsync(
        string workerName, string detailedDescription, string emailOnSuccess, long? parentWorkerID,
        int timeoutMinutes, string directoryName, string executable, string argumentValues, CancellationToken cancellationToken)
    {
        if (!IsValidExecutable(directoryName, executable))
        {
            return new InvalidExecutable();
        }

        DynamicParameters param = new DynamicParameters()
            .AddNVarCharParam("@WorkerName", workerName, 100)
            .AddNVarCharParam("@DetailedDescription", detailedDescription, -1)
            .AddNVarCharParam("@EmailOnSuccess", emailOnSuccess, 100)
            .AddNullableLongParam("@ParentWorkerID", parentWorkerID)
            .AddIntParam("@TimeoutMinutes", timeoutMinutes)
            .AddNVarCharParam("@DirectoryName", directoryName, 1000)
            .AddNVarCharParam("@Executable", executable, 1000)
            .AddNVarCharParam("@ArgumentValues", argumentValues, 1000);

        AddWorkerResult result = await _db.GetOneAsync<AddWorkerResult>(
            "[app].[Workers_Insert]",
            param,
            cancellationToken
        ).ConfigureAwait(false);

        if (result.NameAlreadyExists) { return new NameAlreadyExists(); }
        if (result.CircularReference) { return new CircularReference(); }
        if (!result.Success) { throw new ApplicationException("Invalid call to AddWorker"); }

        return new Success();
    }

    private record class UpdateWorkerResult(bool Success, bool NameAlreadyExists, bool CircularReference);
    async Task<OneOf<Success, InvalidExecutable, NameAlreadyExists, CircularReference>> IWorkerManager.UpdateWorkerAsync(
        long workerID, string workerName, string detailedDescription, string emailOnSuccess,
        long? parentWorkerID, int timeoutMinutes, string directoryName, string executable, string argumentValues,
        CancellationToken cancellationToken)
    {
        if (!IsValidExecutable(directoryName, executable))
        {
            return new InvalidExecutable();
        }

        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", workerID)
            .AddNVarCharParam("@WorkerName", workerName, 100)
            .AddNVarCharParam("@DetailedDescription", detailedDescription, -1)
            .AddNVarCharParam("@EmailOnSuccess", emailOnSuccess, 100)
            .AddNullableLongParam("@ParentWorkerID", parentWorkerID)
            .AddIntParam("@TimeoutMinutes", timeoutMinutes)
            .AddNVarCharParam("@DirectoryName", directoryName, 1000)
            .AddNVarCharParam("@Executable", executable, 1000)
            .AddNVarCharParam("@ArgumentValues", argumentValues, 1000);

        UpdateWorkerResult result = await _db.GetOneAsync<UpdateWorkerResult>(
            "[app].[Workers_Update]",
            param,
            cancellationToken
        ).ConfigureAwait(false);

        if (result.NameAlreadyExists) { return new NameAlreadyExists(); }
        if (result.CircularReference) { return new CircularReference(); }
        if (!result.Success) { throw new ApplicationException("Invalid call to AddWorker"); }

        return new Success();
    }

    async Task IWorkerManager.DeactivateWorkerAsync(long id, CancellationToken cancellationToken)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", id);

        await _db.NonQueryAsync(
            "[app].[Workers_Deactivate]",
            param,
            cancellationToken
        ).ConfigureAwait(false);
    }

    async Task IWorkerManager.ReactivateWorkerAsync(long id, CancellationToken cancellationToken)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", id);

        await _db.NonQueryAsync(
            "[app].[Workers_Reactivate]",
            param,
            cancellationToken
        ).ConfigureAwait(false);
    }

    private bool IsValidExecutable(string directoryName, string executable)
    {
        if (directoryName.Contains('/')
            || executable.Contains('/')
            || directoryName.Contains('\\')
            || executable.Contains('\\'))
        {
            return false;
        }

        string fullPath = Path.Combine(_appSettings.WorkerPath, directoryName, executable);
        if (!File.Exists(fullPath))
        {
            return false;
        }

        return true;
    }
}
