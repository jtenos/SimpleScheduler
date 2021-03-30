using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using SimpleSchedulerData;
using SimpleSchedulerModels;
using SimpleSchedulerModels.Exceptions;

namespace SimpleSchedulerBusiness.Sqlite
{
    public class WorkerManager
        : IWorkerManager
    {
        private readonly DatabaseFactory<SqliteDatabase> _databaseFactory;
        private readonly IServiceProvider _serviceProvider;
        public WorkerManager(DatabaseFactory<SqliteDatabase> databaseFactory, IServiceProvider serviceProvider)
            => (_databaseFactory, _serviceProvider) = (databaseFactory, serviceProvider);

        async Task IWorkerManager.RunNowAsync(int workerID, CancellationToken cancellationToken)
        {
            int scheduleID = await _serviceProvider.GetRequiredService<IScheduleManager>()
                .AddScheduleAsync(workerID, isActive: false,
                sunday: true, monday: true, tuesday: true, wednesday: true, thursday: true,
                friday: true, saturday: true, timeOfDayUTC: TimeSpan.Zero, recurTime: null,
                recurBetweenStartUTC: null, recurBetweenEndUTC: null, oneTime: true, cancellationToken).ConfigureAwait(false);

            await _serviceProvider.GetRequiredService<IJobManager>()
                .AddJobAsync(scheduleID, DateTime.UtcNow, cancellationToken).ConfigureAwait(false);
        }

        public record WorkerIDContainer(int WorkerID);
        async Task<ImmutableArray<int>> IWorkerManager.GetChildWorkerIDsByJobAsync(int jobID, CancellationToken cancellationToken)
        {
            var parameters = new[] { new SqliteParameter("@JobID", jobID) };
            Func<IDataReader, WorkerIDContainer> mapFunc = rdr => new WorkerIDContainer(Convert.ToInt32(rdr[0]));
            const string sql = @"
                ;WITH parent AS (
                    SELECT s.WorkerID
                    FROM [Jobs] j
                    JOIN [Schedules] s ON j.ScheduleID = s.ScheduleID
                    JOIN [Workers] w ON s.WorkerID = w.WorkerID
                    WHERE j.JobID = @JobID
                    AND s.IsActive = 1
                    AND w.IsActive = 1
                )
                SELECT child.WorkerID
                FROM [Workers] child
                JOIN parent ON child.ParentWorkerID = parent.WorkerID
                WHERE child.IsActive = 1;
            ";
            var db = await _databaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            var containers = await db.GetManyAsync<WorkerIDContainer>(sql,
                parameters, mapFunc, cancellationToken).ConfigureAwait(false);
            return containers.Select(x => x.WorkerID).ToImmutableArray();
        }

        async Task<ImmutableArray<Worker>> IWorkerManager.GetAllWorkersAsync(CancellationToken cancellationToken,
            bool getActive, bool getInactive)
        {
            if (!getActive && !getInactive) { return ImmutableArray<Worker>.Empty; }

            var sql = new StringBuilder();
            sql.AppendLine("SELECT * FROM [Workers]");
            if (getActive && getInactive) { sql.Append(";"); }
            else if (getActive) { sql.Append(" WHERE IsActive = 1;"); }
            else if (getInactive) { sql.Append(" WHERE IsActive = 0;"); }
            var db = await _databaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            return (await db.GetManyAsync<Worker>(
                sql.ToString(), ImmutableArray<SqliteParameter>.Empty, MapWorker,
                cancellationToken).ConfigureAwait(false)).ToImmutableArray();
        }

        async Task<ImmutableArray<WorkerDetail>> IWorkerManager.GetAllWorkerDetailsAsync(CancellationToken cancellationToken,
            bool getActive, bool getInactive)
            => await GetWorkerDetailsAsync(
                await ((IWorkerManager)this).GetAllWorkersAsync(cancellationToken, getActive, getInactive).ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);

        async Task<Worker> IWorkerManager.GetWorkerAsync(int workerID, CancellationToken cancellationToken)
        {
            var db = await _databaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            return await db.GetOneAsync<Worker>("SELECT * FROM [Workers] WHERE WorkerID = @WorkerID;",
                ImmutableArray<SqliteParameter>.Empty, MapWorker, cancellationToken).ConfigureAwait(false);
        }

        async Task<int> IWorkerManager.AddWorkerAsync(bool isActive, string workerName,
            string? detailedDescription, string? emailOnSuccess, int? parentWorkerID, int timeoutMinutes, int overdueMinutes,
            string directoryName, string executable, string argumentValues, CancellationToken cancellationToken)
        {
            var db = await _databaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            bool descriptionExists = await db.ScalarAsync<bool>(@"
                    SELECT CASE WHEN EXISTS (
                        SELECT 1 FROM [Workers] WHERE WorkerName = @WorkerName
                    ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
                ", new[] { new SqliteParameter("@WorkerName", workerName) },
                cancellationToken).ConfigureAwait(false);

            if (descriptionExists)
            {
                throw new WorkerAlreadyExistsException(workerName);
            }

            int workerID = await db.ScalarAsync<int>(@"
                INSERT INTO [Workers] (
                    IsActive, UpdateDateTime, WorkerName, DetailedDescription, EmailOnSuccess, ParentWorkerID, TimeoutMinutes, OverdueMinutes
                    , DirectoryName, [Executable], ArgumentValues
                )
                VALUES (
                    @IsActive, @UpdateDateTime, @WorkerName, @DetailedDescription, @EmailOnSuccess, @ParentWorkerID, @TimeoutMinutes, @OverdueMinutes
                    , @DirectoryName, @Executable, @ArgumentValues
                );
                SELECT last_insert_rowid();
            ",
                new[]
                {
                    new SqliteParameter("@IsActive", isActive ? 1 : 0),
                    new SqliteParameter("@UpdateDateTime", long.Parse(DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"))),
                    new SqliteParameter("@WorkerName", workerName),
                    new SqliteParameter("@DetailedDescription", detailedDescription),
                    new SqliteParameter("@EmailOnSuccess", emailOnSuccess),
                    new SqliteParameter("@ParentWorkerID", parentWorkerID ?? (object)DBNull.Value),
                    new SqliteParameter("@TimeoutMinutes", timeoutMinutes),
                    new SqliteParameter("@OverdueMinutes", overdueMinutes),
                    new SqliteParameter("@DirectoryName", directoryName),
                    new SqliteParameter("@Executable", executable),
                    new SqliteParameter("@ArgumentValues", argumentValues)
                }, cancellationToken).ConfigureAwait(false);

            await EnsureNoCircularWorkersAsync(workerID, cancellationToken).ConfigureAwait(false);
            return workerID;
        }

        async Task IWorkerManager.UpdateWorkerAsync(int workerID, bool isActive, string workerName,
            string? detailedDescription, string? emailOnSuccess, int? parentWorkerID, int timeoutMinutes, int overdueMinutes,
            string directoryName, string executable, string argumentValues, CancellationToken cancellationToken)
        {
            var db = await _databaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            await db.NonQueryAsync(@"
                UPDATE [Workers]
                SET
                    IsActive = @IsActive
                    ,UpdateDateTime = @UpdateDateTime
                    ,WorkerName = @WorkerName
                    ,DetailedDescription = @DetailedDescription
                    ,EmailOnSuccess = @EmailOnSuccess
                    ,ParentWorkerID = @ParentWorkerID
                    ,TimeoutMinutes = @TimeoutMinutes
                    ,OverdueMinutes = @OverdueMinutes
                    ,DirectoryName = @DirectoryName
                    ,[Executable] = @Executable
                    ,ArgumentValues = @ArgumentValues
                WHERE WorkerID = @WorkerID;
                ", new[]
                {
                    new SqliteParameter("@WorkerID", workerID),
                    new SqliteParameter("@IsActive", isActive),
                    new SqliteParameter("@UpdateDateTime", long.Parse(DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"))),
                    new SqliteParameter("@WorkerName", workerName),
                    new SqliteParameter("@DetailedDescription", detailedDescription),
                    new SqliteParameter("@EmailOnSuccess", emailOnSuccess),
                    new SqliteParameter("@ParentWorkerID", parentWorkerID ?? (object)DBNull.Value),
                    new SqliteParameter("@TimeoutMinutes", timeoutMinutes),
                    new SqliteParameter("@OverdueMinutes", overdueMinutes),
                    new SqliteParameter("@DirectoryName", directoryName),
                    new SqliteParameter("@Executable", executable),
                    new SqliteParameter("@ArgumentValues", argumentValues)
                }, cancellationToken).ConfigureAwait(false);

            await EnsureNoCircularWorkersAsync(workerID, cancellationToken).ConfigureAwait(false);
        }

        async Task IWorkerManager.DeactivateWorkerAsync(int workerID, CancellationToken cancellationToken)
        {
            var db = await _databaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            await db.NonQueryAsync(@"
                    UPDATE [Workers]
                    SET
                        IsActive = 0
                        ,WorkerName = 'INACTIVE: ' + @FormattedNow + ' ' + WorkerName
                    WHERE WorkerID = @WorkerID;
                ",
               new[]
               {
                   new SqliteParameter("@WorkerID", workerID),
                   new SqliteParameter("@FormattedNow", DateTime.UtcNow.ToString("yyyyMMddHHmmss"))
               }, cancellationToken).ConfigureAwait(false);
        }

        async Task IWorkerManager.ReactivateWorkerAsync(int workerID, CancellationToken cancellationToken)
        {
            var worker = await ((IWorkerManager)this).GetWorkerAsync(workerID, cancellationToken).ConfigureAwait(false);
            if (worker.IsActive) { return; }

            if (Regex.IsMatch(worker.WorkerName, @"^INACTIVE\: [0-9]{14}.*$"))
            {
                string workerName = $"{worker.WorkerName[24..]} (react {DateTime.UtcNow:yyyyMMddHHmmss})";
                await ((IWorkerManager)this).UpdateWorkerAsync(worker.WorkerID, isActive: true, workerName, worker.DetailedDescription,
                    worker.EmailOnSuccess, worker.ParentWorkerID, worker.TimeoutMinutes, worker.OverdueMinutes,
                    worker.DirectoryName, worker.Executable, worker.ArgumentValues, cancellationToken).ConfigureAwait(false);
            }
        }

        private static Worker MapWorker(IDataReader rdr)
            => new Worker(
                Convert.ToInt32(rdr["WorkerID"]),
                rdr["IsActive"] == (object)1,
                (string)rdr["WorkerName"],
                (string)rdr["DetailedDescription"],
                (string)rdr["EmailOnSuccess"],
                rdr.IsDBNull(rdr.GetOrdinal("ParentWorkerID")) ? (int?)null : Convert.ToInt32(rdr["ParentWorkerID"]),
                Convert.ToInt32(rdr["TimeoutMinutes"]),
                Convert.ToInt32(rdr["OverdueMinutes"]),
                (string)rdr["DirectoryName"],
                (string)rdr["Executable"],
                (string)rdr["ArgumentValues"]
            );

        private async Task EnsureNoCircularWorkersAsync(int workerID, CancellationToken cancellationToken)
        {
            var allWorkers = await ((IWorkerManager)this).GetAllWorkersAsync(cancellationToken, getActive: true, getInactive: true);

            int? GetParentWorkerID(int workerID) => allWorkers.Single(x => x.WorkerID == workerID).ParentWorkerID;

            var descendantWorkerIDs = new HashSet<int>();
            descendantWorkerIDs.Add(workerID);

            int? parentWorkerID = GetParentWorkerID(workerID);
            while (parentWorkerID.HasValue)
            {
                if (descendantWorkerIDs.Contains(parentWorkerID.Value))
                {
                    throw new CircularWorkerRelationshipException();
                }
                descendantWorkerIDs.Add(parentWorkerID.Value);
                parentWorkerID = GetParentWorkerID(parentWorkerID.Value);
            }
        }

        private async Task<ImmutableArray<WorkerDetail>> GetWorkerDetailsAsync(IList<Worker> workers,
            CancellationToken cancellationToken)
        {
            var result = new List<WorkerDetail>();
            var allSchedules = await _serviceProvider.GetRequiredService<IScheduleManager>().GetAllSchedulesAsync(
                cancellationToken).ConfigureAwait(false);
            foreach (var worker in workers)
            {
                var schedules = allSchedules.Where(x => x.WorkerID == worker.WorkerID).ToImmutableArray();
                result.Add(new(worker,
                    workers.FirstOrDefault(w => w.WorkerID == worker.ParentWorkerID),
                    schedules));
            }
            return result.ToImmutableArray();
        }
    }
}
