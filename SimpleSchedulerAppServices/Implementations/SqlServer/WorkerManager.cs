using Dapper;
using SimpleSchedulerData;
using SimpleSchedulerDataEntities;

namespace SimpleSchedulerAppServices.Implementations.SqlServer;

/// <summary>
/// SQL Server worker manager. Data access is via stored procedures in the [app] schema.
/// </summary>
public sealed class WorkerManager : WorkerManagerBase
{
    public WorkerManager(IDatabase db)
        : base(db)
    {
    }

    protected override async Task RunNowCoreAsync(long id)
    {
        DynamicParameters param = new DynamicParameters()
            .AddBigIntArrayParam("@WorkerIDs", new[] { id });

        await Db.NonQueryAsync("[app].[Jobs_RunNow]", param).ConfigureAwait(false);
    }

    protected override async Task<WorkerEntity[]> SelectAllCoreAsync(
        string? workerName, string? directoryName, string? executable, bool? activeOnly, bool? inactiveOnly)
    {
        DynamicParameters param = new();
        if (workerName != null) { param.AddNullableNVarCharParam("@WorkerName", workerName, 100); }
        if (directoryName != null) { param.AddNullableNVarCharParam("@DirectoryName", directoryName, 1000); }
        if (executable != null) { param.AddNullableNVarCharParam("@Executable", executable, 1000); }
        if (activeOnly != null) { param.AddNullableBitParam("@ActiveOnly", activeOnly); }
        if (inactiveOnly != null) { param.AddNullableBitParam("@InactiveOnly", inactiveOnly); }

        return await Db.GetManyAsync<WorkerEntity>("[app].[Workers_SelectAll]", param).ConfigureAwait(false);
    }

    protected override async Task<WorkerEntity[]> SelectManyCoreAsync(long[] ids)
    {
        DynamicParameters param = new DynamicParameters().AddBigIntArrayParam("@IDs", ids);
        return await Db.GetManyAsync<WorkerEntity>("[app].[Workers_SelectMany]", param).ConfigureAwait(false);
    }

    protected override async Task<WorkerEntity> SelectCoreAsync(long id)
    {
        DynamicParameters param = new DynamicParameters().AddLongParam("@ID", id);
        return await Db.GetOneAsync<WorkerEntity>("[app].[Workers_Select]", param).ConfigureAwait(false);
    }

    protected override async Task<WorkerWriteResult> InsertWorkerCoreAsync(
        string workerName, string detailedDescription, string emailOnSuccess, long? parentWorkerID,
        int timeoutMinutes, string directoryName, string executable, string argumentValues)
    {
        DynamicParameters param = new DynamicParameters()
            .AddNVarCharParam("@WorkerName", workerName, 100)
            .AddNVarCharParam("@DetailedDescription", detailedDescription, -1)
            .AddNVarCharParam("@EmailOnSuccess", emailOnSuccess, 100)
            .AddNullableLongParam("@ParentWorkerID", parentWorkerID)
            .AddIntParam("@TimeoutMinutes", timeoutMinutes)
            .AddNVarCharParam("@DirectoryName", directoryName, 1000)
            .AddNVarCharParam("@Executable", executable, 1000)
            .AddNVarCharParam("@ArgumentValues", argumentValues, 1000);

        return await Db.GetOneAsync<WorkerWriteResult>("[app].[Workers_Insert]", param).ConfigureAwait(false);
    }

    protected override async Task<WorkerWriteResult> UpdateWorkerCoreAsync(long id,
        string workerName, string detailedDescription, string emailOnSuccess, long? parentWorkerID,
        int timeoutMinutes, string directoryName, string executable, string argumentValues)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", id)
            .AddNVarCharParam("@WorkerName", workerName, 100)
            .AddNVarCharParam("@DetailedDescription", detailedDescription, -1)
            .AddNVarCharParam("@EmailOnSuccess", emailOnSuccess, 100)
            .AddNullableLongParam("@ParentWorkerID", parentWorkerID)
            .AddIntParam("@TimeoutMinutes", timeoutMinutes)
            .AddNVarCharParam("@DirectoryName", directoryName, 1000)
            .AddNVarCharParam("@Executable", executable, 1000)
            .AddNVarCharParam("@ArgumentValues", argumentValues, 1000);

        return await Db.GetOneAsync<WorkerWriteResult>("[app].[Workers_Update]", param).ConfigureAwait(false);
    }

    protected override async Task DeactivateWorkerCoreAsync(long id)
    {
        DynamicParameters param = new DynamicParameters().AddLongParam("@ID", id);
        await Db.NonQueryAsync("[app].[Workers_Deactivate]", param).ConfigureAwait(false);
    }

    protected override async Task ReactivateWorkerCoreAsync(long id)
    {
        DynamicParameters param = new DynamicParameters().AddLongParam("@ID", id);
        await Db.NonQueryAsync("[app].[Workers_Reactivate]", param).ConfigureAwait(false);
    }
}
