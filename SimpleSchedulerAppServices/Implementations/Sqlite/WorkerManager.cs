using Dapper;
using SimpleSchedulerData;
using SimpleSchedulerDataEntities;
using System.Text;

namespace SimpleSchedulerAppServices.Implementations.Sqlite;

/// <summary>
/// SQLite worker manager. Data access is via SQL scripts equivalent to the SQL Server procedures.
/// The circular-reference check (a T-SQL WHILE loop in SQL Server) is expressed as a recursive CTE.
/// </summary>
public sealed class WorkerManager : WorkerManagerBase
{
    // Walks ParentWorkerID upward from @ParentWorkerID; if it reaches @ID, setting @ID's parent to
    // @ParentWorkerID would create a cycle.
    private const string CircularExists =
        "@ParentWorkerID IS NOT NULL AND EXISTS ("
        + "WITH RECURSIVE chain(id) AS ("
        + "  SELECT @ParentWorkerID"
        + "  UNION"
        + "  SELECT w.ParentWorkerID FROM Workers w JOIN chain c ON w.ID = c.id WHERE w.ParentWorkerID IS NOT NULL"
        + ") SELECT 1 FROM chain WHERE id = @ID)";

    private const string NameExists =
        "EXISTS (SELECT 1 FROM Workers WHERE WorkerName = @WorkerName AND ID <> @ID)";

    public WorkerManager(IDatabase db)
        : base(db)
    {
    }

    protected override async Task RunNowCoreAsync(long id)
    {
        DynamicParameters param = new();
        param.Add("@ID", id);
        param.Add("@Now", DateTime.UtcNow);

        await Db.NonQueryAsync(@"
            BEGIN;

            INSERT INTO Schedules (
                IsActive, WorkerID, Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, TimeOfDayUTC, OneTime
            )
            SELECT 0, @ID, 1, 1, 1, 1, 1, 1, 1, '00:00:00', 1
            WHERE @ID NOT IN (SELECT WorkerID FROM Schedules WHERE OneTime = 1);

            INSERT INTO Jobs (ScheduleID, QueueDateUTC)
            SELECT MAX(ID), @Now
            FROM Schedules
            WHERE OneTime = 1 AND WorkerID = @ID
            GROUP BY WorkerID;

            COMMIT;",
            param
        ).ConfigureAwait(false);
    }

    protected override async Task<WorkerEntity[]> SelectAllCoreAsync(
        string? workerName, string? directoryName, string? executable, bool? activeOnly, bool? inactiveOnly)
    {
        StringBuilder sql = new("SELECT * FROM Workers WHERE 1 = 1");
        DynamicParameters param = new();

        if (workerName != null)
        {
            sql.Append(" AND WorkerName LIKE '%' || @WorkerName || '%'");
            param.Add("@WorkerName", workerName);
        }
        if (directoryName != null)
        {
            sql.Append(" AND DirectoryName LIKE '%' || @DirectoryName || '%'");
            param.Add("@DirectoryName", directoryName);
        }
        if (executable != null)
        {
            sql.Append(" AND Executable LIKE '%' || @Executable || '%'");
            param.Add("@Executable", executable);
        }
        if (activeOnly == true) { sql.Append(" AND IsActive = 1"); }
        if (inactiveOnly == true) { sql.Append(" AND IsActive = 0"); }
        sql.Append(';');

        return await Db.GetManyAsync<WorkerEntity>(sql.ToString(), param).ConfigureAwait(false);
    }

    protected override async Task<WorkerEntity[]> SelectManyCoreAsync(long[] ids)
    {
        if (ids.Length == 0) { return Array.Empty<WorkerEntity>(); }

        DynamicParameters param = new();
        param.Add("@IDs", ids);

        return await Db.GetManyAsync<WorkerEntity>(
            "SELECT * FROM Workers WHERE ID IN @IDs;", param).ConfigureAwait(false);
    }

    protected override async Task<WorkerEntity> SelectCoreAsync(long id)
    {
        DynamicParameters param = new();
        param.Add("@ID", id);

        return await Db.GetOneAsync<WorkerEntity>(
            "SELECT * FROM Workers WHERE ID = @ID;", param).ConfigureAwait(false);
    }

    protected override async Task<WorkerWriteResult> InsertWorkerCoreAsync(
        string workerName, string detailedDescription, string emailOnSuccess, long? parentWorkerID,
        int timeoutMinutes, string directoryName, string executable, string argumentValues)
    {
        DynamicParameters param = BuildWorkerParams(workerName, detailedDescription, emailOnSuccess,
            parentWorkerID, timeoutMinutes, directoryName, executable, argumentValues);

        // A brand-new worker can never create a circular reference (nothing points to its new ID),
        // so CircularReference is always 0 here - matching the SQL Server procedure's behavior.
        return await Db.GetOneAsync<WorkerWriteResult>(@"
            INSERT INTO Workers (
                WorkerName, DetailedDescription, EmailOnSuccess, ParentWorkerID,
                TimeoutMinutes, DirectoryName, Executable, ArgumentValues
            )
            SELECT @WorkerName, @DetailedDescription, @EmailOnSuccess, @ParentWorkerID,
                @TimeoutMinutes, @DirectoryName, @Executable, @ArgumentValues
            WHERE NOT EXISTS (SELECT 1 FROM Workers WHERE WorkerName = @WorkerName);

            SELECT
                CASE WHEN changes() = 0 THEN 0 ELSE 1 END AS Success
                ,CASE WHEN changes() = 0 THEN 1 ELSE 0 END AS NameAlreadyExists
                ,0 AS CircularReference;",
            param
        ).ConfigureAwait(false);
    }

    protected override async Task<WorkerWriteResult> UpdateWorkerCoreAsync(long id,
        string workerName, string detailedDescription, string emailOnSuccess, long? parentWorkerID,
        int timeoutMinutes, string directoryName, string executable, string argumentValues)
    {
        DynamicParameters param = BuildWorkerParams(workerName, detailedDescription, emailOnSuccess,
            parentWorkerID, timeoutMinutes, directoryName, executable, argumentValues);
        param.Add("@ID", id);

        // The UPDATE only applies when there is no name conflict and no circular reference; the
        // result SELECT recomputes both conditions to report which (if any) blocked the update.
        return await Db.GetOneAsync<WorkerWriteResult>($@"
            UPDATE Workers SET
                WorkerName = @WorkerName
                ,DetailedDescription = @DetailedDescription
                ,EmailOnSuccess = @EmailOnSuccess
                ,ParentWorkerID = @ParentWorkerID
                ,TimeoutMinutes = @TimeoutMinutes
                ,DirectoryName = @DirectoryName
                ,Executable = @Executable
                ,ArgumentValues = @ArgumentValues
            WHERE ID = @ID
            AND NOT {NameExists}
            AND NOT ({CircularExists});

            WITH flags AS (
                SELECT
                    CASE WHEN {NameExists} THEN 1 ELSE 0 END AS NameExistsFlag
                    ,CASE WHEN {CircularExists} THEN 1 ELSE 0 END AS CircularFlag
            )
            SELECT
                CASE WHEN NameExistsFlag = 1 OR CircularFlag = 1 THEN 0 ELSE 1 END AS Success
                ,NameExistsFlag AS NameAlreadyExists
                ,CircularFlag AS CircularReference
            FROM flags;",
            param
        ).ConfigureAwait(false);
    }

    protected override async Task DeactivateWorkerCoreAsync(long id)
    {
        DynamicParameters param = new();
        param.Add("@ID", id);

        await Db.NonQueryAsync(@"
            BEGIN;

            UPDATE Schedules SET IsActive = 0 WHERE WorkerID = @ID;

            UPDATE Workers
            SET IsActive = 0
                ,WorkerName = trim(substr('INACTIVE: ' || strftime('%Y%m%d%H%M%S', 'now') || ' ' || WorkerName, 1, 100))
            WHERE ID = @ID;

            DELETE FROM Jobs
            WHERE StatusCode = 'NEW'
            AND ScheduleID IN (SELECT ID FROM Schedules WHERE WorkerID = @ID);

            COMMIT;",
            param
        ).ConfigureAwait(false);
    }

    protected override async Task ReactivateWorkerCoreAsync(long id)
    {
        DynamicParameters param = new();
        param.Add("@ID", id);

        await Db.NonQueryAsync(@"
            UPDATE Workers
            SET IsActive = 1
                ,WorkerName = CASE
                    WHEN WorkerName LIKE 'INACTIVE: ______________ %'
                    THEN trim(substr(WorkerName, 26))
                    ELSE WorkerName
                END
            WHERE ID = @ID;",
            param
        ).ConfigureAwait(false);
    }

    private static DynamicParameters BuildWorkerParams(
        string workerName, string detailedDescription, string emailOnSuccess, long? parentWorkerID,
        int timeoutMinutes, string directoryName, string executable, string argumentValues)
    {
        DynamicParameters param = new();
        param.Add("@WorkerName", workerName);
        param.Add("@DetailedDescription", detailedDescription);
        param.Add("@EmailOnSuccess", emailOnSuccess);
        param.Add("@ParentWorkerID", parentWorkerID);
        param.Add("@TimeoutMinutes", timeoutMinutes);
        param.Add("@DirectoryName", directoryName);
        param.Add("@Executable", executable);
        param.Add("@ArgumentValues", argumentValues);
        return param;
    }
}
