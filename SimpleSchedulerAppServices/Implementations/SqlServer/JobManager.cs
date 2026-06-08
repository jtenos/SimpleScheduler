using Dapper;
using Microsoft.Extensions.Logging;
using SimpleSchedulerData;
using SimpleSchedulerDataEntities;
using SimpleSchedulerEmail;

namespace SimpleSchedulerAppServices.Implementations.SqlServer;

/// <summary>
/// SQL Server job manager. Data access is via stored procedures in the [app] schema.
/// </summary>
public sealed class JobManager : JobManagerBase
{
    public JobManager(ILogger<JobManager> logger, IDatabase db, IEmailer emailer)
        : base(logger, db, emailer)
    {
    }

    protected override async Task RestartStuckJobsCoreAsync()
        => await Db.NonQueryAsync("[app].[Jobs_RestartStuck]", parameters: null).ConfigureAwait(false);

    protected override async Task AcknowledgeErrorCoreAsync(Guid acknowledgementCode)
    {
        DynamicParameters param = new DynamicParameters()
            .AddUniqueIdentifierParam("@AcknowledgementCode", acknowledgementCode);

        await Db.NonQueryAsync("[app].[Jobs_AcknowledgeError]", param).ConfigureAwait(false);
    }

    protected override async Task<JobEntity> GetJobEntityAsync(long id)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", id);

        return await Db.GetOneAsync<JobEntity>("[app].[Jobs_Select]", param).ConfigureAwait(false);
    }

    protected override async Task<CancelJobResult> CancelJobCoreAsync(long jobID)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", jobID);

        return await Db.GetOneAsync<CancelJobResult>("[app].[Jobs_Cancel]", param).ConfigureAwait(false);
    }

    protected override async Task CompleteJobCoreAsync(long id, bool success, bool hasDetailedMessage)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", id)
            .AddBitParam("@Success", success)
            .AddBitParam("@HasDetailedMessage", hasDetailedMessage);

        await Db.NonQueryAsync("[app].[Jobs_Complete]", param).ConfigureAwait(false);
    }

    protected override async Task<(JobEntity[] Jobs, WorkerEntity[] Workers)> GetJobWithWorkerCoreAsync(long id)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", id);

        return await Db.GetManyAsync<JobEntity, WorkerEntity>(
            "[app].[JobsWithWorker_Select]", param).ConfigureAwait(false);
    }

    protected override async Task<JobWithWorkerIDEntity[]> SearchJobsCoreAsync(
        string? statusCode, long? workerID, string? workerName, bool overdueOnly, int offset, int numRows)
    {
        DynamicParameters param = new DynamicParameters()
            .AddNullableNCharParam("@StatusCode", statusCode, 3)
            .AddNullableLongParam("@WorkerID", workerID)
            .AddNullableNVarCharParam("@WorkerName", workerName, 100)
            .AddBitParam("@OverdueOnly", overdueOnly)
            .AddIntParam("@Offset", offset)
            .AddIntParam("@NumRows", numRows);

        return await Db.GetManyAsync<JobWithWorkerIDEntity>("[app].[Jobs_Search]", param).ConfigureAwait(false);
    }

    protected override async Task<ScheduleEntity[]> GetSchedulesByIdsAsync(long[] ids)
    {
        DynamicParameters param = new DynamicParameters()
            .AddBigIntArrayParam("@IDs", ids);

        return await Db.GetManyAsync<ScheduleEntity>("[app].[Schedules_SelectMany]", param).ConfigureAwait(false);
    }

    protected override async Task<WorkerEntity[]> GetWorkersByIdsAsync(long[] ids)
    {
        DynamicParameters param = new DynamicParameters()
            .AddBigIntArrayParam("@IDs", ids);

        return await Db.GetManyAsync<WorkerEntity>("[app].[Workers_SelectMany]", param).ConfigureAwait(false);
    }

    protected override async Task<(JobWithWorkerIDEntity[] Jobs, WorkerEntity[] Workers)> DequeueCoreAsync()
        => await Db.GetManyAsync<JobWithWorkerIDEntity, WorkerEntity>(
            "[app].[Jobs_Dequeue]", parameters: null).ConfigureAwait(false);

    protected override async Task<ScheduleEntity[]> GetSchedulesForJobInsertionAsync()
        => await Db.GetManyAsync<ScheduleEntity>(
            "[app].[Schedules_SelectForJobInsertion]", parameters: null).ConfigureAwait(false);

    protected override async Task<JobEntity?> GetMostRecentJobByScheduleAsync(long scheduleID)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ScheduleID", scheduleID);

        return await Db.GetZeroOrOneAsync<JobEntity>(
            "[app].[Jobs_SelectMostRecentBySchedule]", param).ConfigureAwait(false);
    }

    protected override async Task InsertJobCoreAsync(long scheduleID, DateTime queueDateUTC)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ScheduleID", scheduleID)
            .AddDateTimeParam("@QueueDateUTC", queueDateUTC);

        await Db.NonQueryAsync("[app].[Jobs_Insert]", param).ConfigureAwait(false);
    }
}
