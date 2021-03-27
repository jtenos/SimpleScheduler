using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SimpleSchedulerData;
using SimpleSchedulerModels;
using SimpleSchedulerModels.Exceptions;

namespace SimpleSchedulerBusiness.Sqlite
{
    public class WorkerManager
        : BaseManager, IWorkerManager
    {
        public WorkerManager(IDatabaseFactory databaseFactory, IServiceProvider serviceProvider)
            : base(databaseFactory, serviceProvider) { }

        async Task IWorkerManager.RunNowAsync(int workerID, CancellationToken cancellationToken)
        {
            int scheduleID = await GetScheduleManager().AddScheduleAsync(workerID, isActive: false,
                sunday: true, monday: true, tuesday: true, wednesday: true, thursday: true,
                friday: true, saturday: true, timeOfDayUTC: TimeSpan.Zero, recurTime: null,
                recurBetweenStartUTC: null, recurBetweenEndUTC: null, oneTime: true, cancellationToken).ConfigureAwait(false);

            await GetJobManager().AddJobAsync(scheduleID, DateTime.UtcNow, cancellationToken).ConfigureAwait(false);
        }

        public record WorkerIDContainer(int WorkerID);
        async Task<ImmutableArray<int>> IWorkerManager.GetChildWorkerIDsByJobAsync(int jobID, CancellationToken cancellationToken)
            => (await GetManyAsync<WorkerIDContainer>(@"
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
            ", CreateDynamicParameters()
                .AddIntParam("@JobID", jobID), cancellationToken).ConfigureAwait(false))
            .Select(x => x.WorkerID).ToImmutableArray();

        async Task<ImmutableArray<Worker>> IWorkerManager.GetAllWorkersAsync(CancellationToken cancellationToken,
            bool getActive, bool getInactive)
        {
            if (!getActive && !getInactive) { return ImmutableArray<Worker>.Empty; }

            var sql = new StringBuilder();
            sql.AppendLine("SELECT * FROM [Workers]");
            var allWorkers = await GetManyAsync<Worker>(sql.ToString(), CreateDynamicParameters(), cancellationToken).ConfigureAwait(false);
            return allWorkers
                .Where(w => (getActive && w.IsActive) || (getInactive && !w.IsActive))
                .ToImmutableArray();
        }

        async Task<ImmutableArray<WorkerDetail>> IWorkerManager.GetAllWorkerDetailsAsync(CancellationToken cancellationToken,
            bool getActive, bool getInactive)
            => await GetWorkerDetailsAsync(
                await ((IWorkerManager)this).GetAllWorkersAsync(cancellationToken, getActive, getInactive).ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);

        async Task<Worker> IWorkerManager.GetWorkerAsync(int workerID, CancellationToken cancellationToken)
            => await GetOneAsync<Worker>("SELECT * FROM [Workers] WHERE WorkerID = @WorkerID;",
                CreateDynamicParameters()
                .AddIntParam("@WorkerID", workerID), cancellationToken).ConfigureAwait(false);

        async Task<int> IWorkerManager.AddWorkerAsync(bool isActive, string workerName,
            string? detailedDescription, string? emailOnSuccess, int? parentWorkerID, int timeoutMinutes, int overdueMinutes,
            string directoryName, string executable, string argumentValues, CancellationToken cancellationToken)
        {
            bool descriptionExists = await ScalarAsync<bool>(@"
                    SELECT CASE WHEN EXISTS (
                        SELECT 1 FROM [Workers] WHERE WorkerName = @WorkerName
                    ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
                ", CreateDynamicParameters()
                .AddNVarCharParam("@WorkerName", workerName, 100),
                cancellationToken).ConfigureAwait(false);

            if (descriptionExists)
            {
                throw new WorkerAlreadyExistsException(workerName);
            }

            int workerID = (await ScalarAsync<int>(@"
                INSERT INTO [Workers] (
                    IsActive, WorkerName, DetailedDescription, EmailOnSuccess, ParentWorkerID, TimeoutMinutes, OverdueMinutes
                    , DirectoryName, [Executable], ArgumentValues
                )
                VALUES (
                    @IsActive, @WorkerName, @DetailedDescription, @EmailOnSuccess, @ParentWorkerID, @TimeoutMinutes, @OverdueMinutes
                    , @DirectoryName, @Executable, @ArgumentValues
                );
                SELECT last_insert_rowid();
            ",
                CreateDynamicParameters()
                .AddBitParam("@IsActive", isActive)
                .AddNVarCharParam("@WorkerName", workerName, 100)
                .AddNullableNVarCharParam("@DetailedDescription", detailedDescription, -1)
                .AddNullableNVarCharParam("@EmailOnSuccess", emailOnSuccess, -1)
                .AddNullableIntParam("@ParentWorkerID", parentWorkerID)
                .AddIntParam("@TimeoutMinutes", timeoutMinutes)
                .AddIntParam("@OverdueMinutes", overdueMinutes)
                .AddNVarCharParam("@DirectoryName", directoryName, 1000)
                .AddNVarCharParam("@Executable", executable, 1000)
                .AddNVarCharParam("@ArgumentValues", argumentValues, 1000),
                cancellationToken).ConfigureAwait(false));

            await EnsureNoCircularWorkersAsync(workerID, cancellationToken).ConfigureAwait(false);
            return workerID;
        }

        async Task IWorkerManager.UpdateWorkerAsync(int workerID, bool isActive, string workerName,
            string? detailedDescription, string? emailOnSuccess, int? parentWorkerID, int timeoutMinutes, int overdueMinutes,
            string directoryName, string executable, string argumentValues, CancellationToken cancellationToken)
        {
            await NonQueryAsync(@"
                UPDATE [Workers]
                SET
                    IsActive = @IsActive
                    ,UpdateDateTime = @Now
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
                ",
                CreateDynamicParameters()
                .AddIntParam("@WorkerID", workerID)
                .AddBitParam("@IsActive", isActive)
                .AddDateTime2Param("@Now", DateTime.UtcNow)
                .AddNVarCharParam("@WorkerName", workerName, 100)
                .AddNullableNVarCharParam("@DetailedDescription", detailedDescription, -1)
                .AddNullableNVarCharParam("@EmailOnSuccess", emailOnSuccess, -1)
                .AddNullableIntParam("@ParentWorkerID", parentWorkerID)
                .AddIntParam("@TimeoutMinutes", timeoutMinutes)
                .AddIntParam("@OverdueMinutes", overdueMinutes)
                .AddNVarCharParam("@DirectoryName", directoryName, 1000)
                .AddNVarCharParam("@Executable", executable, 1000)
                .AddNVarCharParam("@ArgumentValues", argumentValues, 1000),
                cancellationToken).ConfigureAwait(false);

            await EnsureNoCircularWorkersAsync(workerID, cancellationToken).ConfigureAwait(false);
        }

        async Task IWorkerManager.DeactivateWorkerAsync(int workerID, CancellationToken cancellationToken)
            => await NonQueryAsync(@"
                    UPDATE [Workers]
                    SET
                        IsActive = 0
                        ,WorkerName = 'INACTIVE: ' + @FormattedNow + ' ' + WorkerName
                    WHERE WorkerID = @WorkerID;
                ",
                CreateDynamicParameters()
                .AddIntParam("@WorkerID", workerID)
                .AddNVarCharParam("@FormattedNow", DateTime.UtcNow.ToString("yyyyMMddHHmmss"), 14),
                cancellationToken).ConfigureAwait(false);

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
            var allSchedules = await GetScheduleManager().GetAllSchedulesAsync(
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
