using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using SimpleSchedulerData;
using SimpleSchedulerModels;
using SimpleSchedulerModels.Exceptions;

namespace SimpleSchedulerBusiness.Sqlite
{
    public class JobManager
        : BaseManager, IJobManager
    {
        public JobManager(IDatabaseFactory databaseFactory, IServiceProvider serviceProvider)
            : base(databaseFactory, serviceProvider) { }

        async Task IJobManager.RestartStuckJobsAsync(CancellationToken cancellationToken)
            => await NonQueryAsync(@"
                UPDATE [Jobs] SET StatusCode = 'NEW' WHERE StatusCode = 'RUN';
            ", CreateDynamicParameters(), cancellationToken).ConfigureAwait(false);

        async Task IJobManager.AcknowledgeErrorAsync(int jobID, CancellationToken cancellationToken)
            => await NonQueryAsync(@"
                UPDATE [Jobs] SET StatusCode = 'ACK' WHERE JobID = @JobID AND StatusCode = 'ERR';
            ", CreateDynamicParameters()
                .AddIntParam("@JobID", jobID), cancellationToken).ConfigureAwait(false);

        async Task IJobManager.AddJobAsync(int scheduleID, DateTime queueDateUTC, CancellationToken cancellationToken)
            => await NonQueryAsync(@"
                INSERT INTO [Jobs] (ScheduleID, QueueDateUTC)
                VALUES (@ScheduleID, @QueueDateUTC)
            ", CreateDynamicParameters()
                .AddIntParam("@ScheduleID", scheduleID)
                .AddDateTime2Param("@QueueDateUTC", queueDateUTC),
                cancellationToken).ConfigureAwait(false);

        async Task<Job> IJobManager.GetJobAsync(int jobID, CancellationToken cancellationToken)
            => await GetOneAsync<Job>(@"
                SELECT * FROM [Jobs] WHERE JobID = @JobID;
            ",
                CreateDynamicParameters()
                .AddIntParam("@JobID", jobID),
                cancellationToken).ConfigureAwait(false);

        async Task IJobManager.CancelJobAsync(int jobID, CancellationToken cancellationToken)
        {
            int numRecords = await NonQueryAsync(@"
                UPDATE [Jobs]
                SET StatusCode = 'CAN', UpdateDateTime = @Now
                WHERE JobID = @JobID
                AND StatusCode = 'NEW';
            ", CreateDynamicParameters()
                .AddIntParam("@JobID", jobID)
                .AddDateTime2Param("@Now", DateTime.UtcNow), cancellationToken).ConfigureAwait(false);

            if (numRecords == 1)
            {
                return;
            }

            var job = await ((IJobManager)this).GetJobAsync(jobID, cancellationToken).ConfigureAwait(false);
            if (new[] { "ERR", "ACK", "SUC" }.Contains(job?.StatusCode))
            {
                throw new JobAlreadyCompletedException();
            }
            if (job?.StatusCode == "RUN")
            {
                throw new JobAlreadyRunningException();
            }
        }

        async Task IJobManager.CompleteJobAsync(int jobID, string statusCode, string? detailedMessage,
            CancellationToken cancellationToken)
        {
            await NonQueryAsync(@"
                UPDATE [Jobs]
                SET UpdateDateTime = @Now, StatusCode = @StatusCode, DetailedMessage = @DetailedMessage, CompleteDateUTC = @Now
                WHERE JobID = @JobID;
            ",
                CreateDynamicParameters()
                .AddDateTime2Param("@Now", DateTime.UtcNow)
                .AddNCharParam("@StatusCode", statusCode, 3)
                .AddNullableNVarCharParam("@DetailedMessage", detailedMessage, -1)
                .AddIntParam("@JobID", jobID)
                .AddDateTime2Param("@Now", DateTime.UtcNow),
                cancellationToken).ConfigureAwait(false);

            if (statusCode == "SUC")
            {
                var workerManager = GetWorkerManager();
                var childWorkerIDs = await workerManager.GetChildWorkerIDsByJobAsync(jobID, cancellationToken).ConfigureAwait(false);
                foreach (int childWorkerID in childWorkerIDs)
                {
                    await workerManager.RunNowAsync(childWorkerID, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        async Task<ImmutableArray<JobDetail>> IJobManager.GetLatestJobsAsync(
            int pageNumber, int rowsPerPage, CancellationToken cancellationToken,
            string? statusCode, int? workerID, bool overdueOnly)
        {
            var parms = new DynamicParameters();
            parms.AddDateTime2Param("@Now", DateTime.UtcNow);

            var sql = new StringBuilder();
            sql.AppendLine("SELECT * FROM [Jobs] WHERE 1=1");
            if (statusCode != null)
            {
                sql.AppendLine("AND StatusCode = @StatusCode");
                parms.AddNCharParam("@StatusCode", statusCode, 3);
            }
            if (workerID.HasValue)
            {
                sql.AppendLine("AND ScheduleID IN (SELECT ScheduleID FROM [Schedules] WHERE WorkerID = @WorkerID)");
                parms.AddIntParam("@WorkerID", workerID.Value);
            }
            if (overdueOnly)
            {
                sql.AppendLine("AND StatusCode IN ('ERR', 'NEW', 'RUN')");
                sql.AppendLine(@"AND CASE StatusCode
                                    WHEN 'RUN' THEN DATEADD(MINUTE, OverdueMinutes, QueueDateUTC)
                                    WHEN 'ERR' THEN QueueDateUTC
                                    WHEN 'NEW' THEN DATEADD(MINUTE, 1, QueueDateUTC)
                                END < @Now");
            }
            sql.AppendLine("ORDER BY QueueDateUTC DESC");
            sql.AppendLine($"LIMIT {rowsPerPage} OFFSET {pageNumber * rowsPerPage};");

            var jobs = await GetManyAsync<Job>(sql.ToString(), parms, cancellationToken).ConfigureAwait(false);

            return await GetJobDetailsAsync(jobs, cancellationToken).ConfigureAwait(false);
        }

        async Task<ImmutableArray<JobDetail>> IJobManager.GetOverdueJobsAsync(CancellationToken cancellationToken)
            => (await ((IJobManager)this).GetLatestJobsAsync(pageNumber: 1, rowsPerPage: 999, cancellationToken, overdueOnly: true).ConfigureAwait(false))
                .OrderBy(x => x.Job.QueueDateUTC)
                .ToImmutableArray();

        async Task<Job?> IJobManager.GetLastQueuedJobAsync(int scheduleID, CancellationToken cancellationToken)
            => (await GetManyAsync<Job>(@"
                SELECT * FROM [Jobs]
                WHERE ScheduleID = @ScheduleID
                ORDER BY QueueDateUTC DESC
                LIMIT 1 OFFSET 0;
            ", CreateDynamicParameters()
                .AddIntParam("@ScheduleID", scheduleID),
                cancellationToken).ConfigureAwait(false)).FirstOrDefault();

        async Task<string?> IJobManager.GetJobDetailedMessageAsync(int jobID, CancellationToken cancellationToken)
            => await ScalarAsync<string>(@"
                SELECT DetailedMessage FROM [Jobs] WHERE JobID = @JobID;
            ", CreateDynamicParameters()
                .AddIntParam("@JobID", jobID), cancellationToken).ConfigureAwait(false);

        async Task<ImmutableArray<JobDetail>> IJobManager.DequeueScheduledJobsAsync(CancellationToken cancellationToken)
        {
            var dequeuedJobs = await GetManyAsync<Job>(@"
                CREATE TABLE temp.Result(JobID INTEGER, StatusCode TEXT);
                INSERT INTO temp.Result
                    SELECT JobID
                    FROM [Jobs]
                    WHERE StatusCode = 'NEW'
                    AND QueueDateUTC < @Now
                    ORDER BY QueueDateUTC
                    LIMIT 3 OFFSET 0;

                UPDATE [Jobs]
                SET StatusCode = 'RUN'
                WHERE JobID in (SELECT JobID FROM temp.Result);

                SELECT j.* 
                FROM Jobs j
                JOIN temp.Result r on j.JobID = r.JobID;
            ",
                CreateDynamicParameters()
                .AddDateTime2Param("@Now", DateTime.UtcNow),
                cancellationToken).ConfigureAwait(false);

            if (!dequeuedJobs.Any())
            {
                return ImmutableArray<JobDetail>.Empty;
            }

            return await GetJobDetailsAsync(dequeuedJobs, cancellationToken).ConfigureAwait(false);
        }

        private async Task<ImmutableArray<JobDetail>> GetJobDetailsAsync(IEnumerable<Job> jobs, CancellationToken cancellationToken)
        {
            var allSchedules = await GetScheduleManager().GetAllSchedulesAsync(cancellationToken).ConfigureAwait(false);
            var allWorkers = await GetWorkerManager().GetAllWorkersAsync(cancellationToken).ConfigureAwait(false);

            var result = new List<JobDetail>();
            foreach (var job in jobs)
            {
                var schedule = allSchedules.SingleOrDefault(s => s.ScheduleID == job.ScheduleID)
                    ?? (await GetScheduleManager().GetScheduleAsync(job.ScheduleID, cancellationToken).ConfigureAwait(false)).Schedule;
                var worker = allWorkers.SingleOrDefault(w => w.WorkerID == schedule.WorkerID)
                    ?? await GetWorkerManager().GetWorkerAsync(schedule.WorkerID, cancellationToken).ConfigureAwait(false);
                result.Add(new(job, schedule, worker));
            }
            return result.ToImmutableArray();
        }
    }
}
