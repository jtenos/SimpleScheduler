using Dapper;
using Microsoft.Extensions.Logging;
using SimpleSchedulerData;
using SimpleSchedulerDataEntities;
using SimpleSchedulerEmail;
using System.Text;

namespace SimpleSchedulerAppServices.Implementations.Sqlite;

/// <summary>
/// SQLite job manager. Data access is via SQL scripts (the SQLite equivalents of the SQL Server
/// stored procedures), executed as text commands. See the repo notes for translation specifics
/// (RETURNING, recursive CTEs, list params instead of TVPs, etc.).
/// </summary>
public sealed class JobManager : JobManagerBase
{
    public JobManager(ILogger<JobManager> logger, IDatabase db, IEmailer emailer)
        : base(logger, db, emailer)
    {
    }

    private sealed record StatusRow(string StatusCode);

    protected override async Task RestartStuckJobsCoreAsync()
        => await Db.NonQueryAsync("UPDATE Jobs SET StatusCode = 'NEW' WHERE StatusCode = 'RUN';", null)
            .ConfigureAwait(false);

    protected override async Task AcknowledgeErrorCoreAsync(Guid acknowledgementCode)
    {
        DynamicParameters param = new();
        param.Add("@AcknowledgementCode", acknowledgementCode);

        StatusRow? existing = await Db.GetZeroOrOneAsync<StatusRow>(
            "SELECT StatusCode FROM Jobs WHERE AcknowledgementCode = @AcknowledgementCode;",
            param
        ).ConfigureAwait(false);

        if (existing is null) { throw new ApplicationException("Job not found"); }
        if (existing.StatusCode == "ACK") { throw new ApplicationException("Error already acknowledged"); }

        await Db.NonQueryAsync(
            "UPDATE Jobs SET StatusCode = 'ACK' WHERE AcknowledgementCode = @AcknowledgementCode AND StatusCode = 'ERR';",
            param
        ).ConfigureAwait(false);
    }

    protected override async Task<JobEntity> GetJobEntityAsync(long id)
    {
        DynamicParameters param = new();
        param.Add("@ID", id);

        return await Db.GetOneAsync<JobEntity>(
            "SELECT * FROM Jobs WHERE ID = @ID;", param).ConfigureAwait(false);
    }

    protected override async Task<CancelJobResult> CancelJobCoreAsync(long jobID)
    {
        DynamicParameters param = new();
        param.Add("@ID", jobID);

        return await Db.GetOneAsync<CancelJobResult>(@"
            UPDATE Jobs SET StatusCode = 'CAN' WHERE ID = @ID AND StatusCode = 'NEW';

            SELECT
                CASE WHEN StatusCode = 'CAN' THEN 1 ELSE 0 END AS Success
                ,CASE WHEN StatusCode IN ('ERR', 'ACK', 'SUC') THEN 1 ELSE 0 END AS AlreadyCompleted
                ,CASE WHEN StatusCode IN ('ERR', 'ACK', 'SUC', 'RUN') THEN 1 ELSE 0 END AS AlreadyStarted
            FROM Jobs
            WHERE ID = @ID;",
            param
        ).ConfigureAwait(false);
    }

    protected override async Task CompleteJobCoreAsync(long id, bool success, bool hasDetailedMessage)
    {
        DynamicParameters param = new();
        param.Add("@ID", id);
        param.Add("@Success", success);
        param.Add("@HasDetailedMessage", hasDetailedMessage);
        param.Add("@Now", DateTime.UtcNow);

        // Updates the job, then (on success) creates one-time schedules + jobs for active child
        // workers (the SQLite equivalent of Jobs_Complete calling Jobs_RunNow).
        await Db.NonQueryAsync(@"
            BEGIN;

            UPDATE Jobs
            SET StatusCode = CASE @Success WHEN 1 THEN 'SUC' ELSE 'ERR' END
                ,HasDetailedMessage = @HasDetailedMessage
                ,CompleteDateUTC = CASE @Success WHEN 1 THEN @Now END
            WHERE ID = @ID;

            INSERT INTO Schedules (
                IsActive, WorkerID, Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, TimeOfDayUTC, OneTime
            )
            SELECT 0, w.ID, 1, 1, 1, 1, 1, 1, 1, '00:00:00', 1
            FROM Workers w
            WHERE @Success = 1
            AND w.IsActive = 1
            AND w.ParentWorkerID = (SELECT s.WorkerID FROM Jobs j JOIN Schedules s ON j.ScheduleID = s.ID WHERE j.ID = @ID)
            AND w.ID NOT IN (SELECT WorkerID FROM Schedules WHERE OneTime = 1);

            INSERT INTO Jobs (ScheduleID, QueueDateUTC)
            SELECT MAX(s.ID), @Now
            FROM Schedules s
            WHERE @Success = 1
            AND s.OneTime = 1
            AND s.WorkerID IN (
                SELECT w.ID FROM Workers w
                WHERE w.IsActive = 1
                AND w.ParentWorkerID = (SELECT s2.WorkerID FROM Jobs j JOIN Schedules s2 ON j.ScheduleID = s2.ID WHERE j.ID = @ID)
            )
            GROUP BY s.WorkerID;

            COMMIT;",
            param
        ).ConfigureAwait(false);
    }

    protected override async Task<(JobEntity[] Jobs, WorkerEntity[] Workers)> GetJobWithWorkerCoreAsync(long id)
    {
        DynamicParameters param = new();
        param.Add("@ID", id);

        return await Db.GetManyAsync<JobEntity, WorkerEntity>(@"
            SELECT * FROM Jobs WHERE ID = @ID;

            SELECT w.* FROM Workers w
            JOIN Schedules s ON w.ID = s.WorkerID
            JOIN Jobs j ON s.ID = j.ScheduleID
            WHERE j.ID = @ID;",
            param
        ).ConfigureAwait(false);
    }

    protected override async Task<JobWithWorkerIDEntity[]> SearchJobsCoreAsync(
        string? statusCode, long? workerID, string? workerName, bool overdueOnly, int offset, int numRows)
    {
        StringBuilder sql = new("SELECT * FROM JobsWithWorkerID WHERE 1 = 1");
        DynamicParameters param = new();

        if (statusCode is not null)
        {
            sql.Append(" AND StatusCode = @StatusCode");
            param.Add("@StatusCode", statusCode);
        }
        if (workerID is not null)
        {
            sql.Append(" AND WorkerID = @WorkerID");
            param.Add("@WorkerID", workerID);
        }
        if (workerName is not null)
        {
            sql.Append(" AND WorkerName LIKE '%' || @WorkerName || '%'");
            param.Add("@WorkerName", workerName);
        }
        if (overdueOnly)
        {
            sql.Append(" AND StatusCode IN ('ERR', 'NEW', 'RUN')");
        }

        sql.Append(" ORDER BY QueueDateUTC DESC LIMIT @NumRows OFFSET @Offset;");
        param.Add("@NumRows", numRows);
        param.Add("@Offset", offset);

        return await Db.GetManyAsync<JobWithWorkerIDEntity>(sql.ToString(), param).ConfigureAwait(false);
    }

    protected override async Task<ScheduleEntity[]> GetSchedulesByIdsAsync(long[] ids)
    {
        if (ids.Length == 0) { return Array.Empty<ScheduleEntity>(); }

        DynamicParameters param = new();
        param.Add("@IDs", ids);

        return await Db.GetManyAsync<ScheduleEntity>(
            "SELECT * FROM Schedules WHERE ID IN @IDs;", param).ConfigureAwait(false);
    }

    protected override async Task<WorkerEntity[]> GetWorkersByIdsAsync(long[] ids)
    {
        if (ids.Length == 0) { return Array.Empty<WorkerEntity>(); }

        DynamicParameters param = new();
        param.Add("@IDs", ids);

        return await Db.GetManyAsync<WorkerEntity>(
            "SELECT * FROM Workers WHERE ID IN @IDs;", param).ConfigureAwait(false);
    }

    protected override async Task<(JobWithWorkerIDEntity[] Jobs, WorkerEntity[] Workers)> DequeueCoreAsync()
    {
        DynamicParameters param = new();
        param.Add("@Now", DateTime.UtcNow);

        // Mark up to five ready jobs as RUN (one per worker, skipping workers already running),
        // then return the started jobs and their workers. Equivalent to Jobs_Dequeue.
        return await Db.GetManyAsync<JobWithWorkerIDEntity, WorkerEntity>(@"
            DROP TABLE IF EXISTS _dequeue;

            CREATE TEMP TABLE _dequeue AS
            SELECT MIN(JobID) AS JobID
            FROM (
                SELECT j.ID AS JobID, s.WorkerID AS WorkerID
                FROM Jobs j
                JOIN Schedules s ON j.ScheduleID = s.ID
                WHERE j.StatusCode = 'NEW'
                AND j.QueueDateUTC < @Now
                AND s.WorkerID NOT IN (
                    SELECT s1.WorkerID
                    FROM Jobs j1
                    JOIN Schedules s1 ON j1.ScheduleID = s1.ID
                    WHERE j1.StatusCode = 'RUN'
                )
                ORDER BY j.QueueDateUTC
                LIMIT 5
            ) cand
            GROUP BY WorkerID;

            UPDATE Jobs SET StatusCode = 'RUN' WHERE ID IN (SELECT JobID FROM _dequeue);

            SELECT * FROM JobsWithWorkerID WHERE ID IN (SELECT JobID FROM _dequeue);

            SELECT * FROM Workers WHERE ID IN (
                SELECT s.WorkerID
                FROM Schedules s
                JOIN Jobs j ON s.ID = j.ScheduleID
                WHERE j.ID IN (SELECT JobID FROM _dequeue)
            );

            DROP TABLE IF EXISTS _dequeue;",
            param
        ).ConfigureAwait(false);
    }

    protected override async Task<ScheduleEntity[]> GetSchedulesForJobInsertionAsync()
        => await Db.GetManyAsync<ScheduleEntity>(@"
            SELECT * FROM Schedules
            WHERE IsActive = 1
            AND ID NOT IN (
                SELECT ScheduleID FROM Jobs WHERE StatusCode IN ('NEW', 'RUN')
            );",
            parameters: null
        ).ConfigureAwait(false);

    protected override async Task<JobEntity?> GetMostRecentJobByScheduleAsync(long scheduleID)
    {
        DynamicParameters param = new();
        param.Add("@ScheduleID", scheduleID);

        return await Db.GetZeroOrOneAsync<JobEntity>(
            "SELECT * FROM Jobs WHERE ScheduleID = @ScheduleID ORDER BY QueueDateUTC DESC LIMIT 1;",
            param
        ).ConfigureAwait(false);
    }

    protected override async Task InsertJobCoreAsync(long scheduleID, DateTime queueDateUTC)
    {
        DynamicParameters param = new();
        param.Add("@ScheduleID", scheduleID);
        param.Add("@QueueDateUTC", queueDateUTC);

        await Db.NonQueryAsync(
            "INSERT INTO Jobs (ScheduleID, QueueDateUTC) VALUES (@ScheduleID, @QueueDateUTC);",
            param
        ).ConfigureAwait(false);
    }
}
