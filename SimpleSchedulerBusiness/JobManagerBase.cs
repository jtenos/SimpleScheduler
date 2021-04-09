using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleSchedulerData;
using SimpleSchedulerEntities;
using SimpleSchedulerModels;
using SimpleSchedulerModels.Exceptions;

namespace SimpleSchedulerBusiness
{
    public abstract class JobManagerBase
        : IJobManager
    {
        protected JobManagerBase(DatabaseFactory databaseFactory, IServiceProvider serviceProvider)
            => (DatabaseFactory, ServiceProvider) = (databaseFactory, serviceProvider);

        protected DatabaseFactory DatabaseFactory { get; }
        protected IServiceProvider ServiceProvider { get; }

        public virtual async Task AcknowledgeErrorAsync(long jobID, CancellationToken cancellationToken)
        {
            var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            DbParameter[] parms =
            {
                db.GetInt64Parameter("@JobID", jobID)
            };
            await db.NonQueryAsync(@"
                UPDATE [Jobs] SET StatusCode = 'ACK' WHERE JobID = @JobID AND StatusCode = 'ERR';
            ", parms, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task AddJobAsync(long scheduleID, DateTime queueDateUTC, CancellationToken cancellationToken)
        {
            var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            var parms = new[]
            {
                db.GetInt64Parameter("@ScheduleID", scheduleID),
                db.GetInt64Parameter("@QueueDateUTC", queueDateUTC),
                db.GetInt64Parameter("@InsertDateUTC", DateTime.UtcNow),
                db.GetStringParameter("@AcknowledgementID", Guid.NewGuid().ToString("N"), isFixed: true, size: 32)
            };
            await db.NonQueryAsync(@"
                INSERT INTO Jobs (ScheduleID, QueueDateUTC, InsertDateUTC, AcknowledgementID)
                VALUES (@ScheduleID, @QueueDateUTC, @InsertDateUTC, @AcknowledgementID)
            ", parms, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task CancelJobAsync(long jobID, CancellationToken cancellationToken)
        {
            var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            DbParameter[] parms =
            {
                db.GetInt64Parameter("@JobID", jobID)
            };
            int numRecords = await db.NonQueryAsync(@"
                UPDATE [Jobs]
                SET StatusCode = 'CAN'
                WHERE JobID = @JobID
                AND StatusCode = 'NEW';
            ", parms, cancellationToken).ConfigureAwait(false);

            if (numRecords == 1)
            {
                return;
            }

            var job = await GetJobAsync(jobID, cancellationToken).ConfigureAwait(false);
            if (new[] { "ERR", "ACK", "SUC" }.Contains(job.StatusCode))
            {
                throw new JobAlreadyCompletedException();
            }
            if (job.StatusCode == "RUN")
            {
                throw new JobAlreadyRunningException();
            }
        }

        public virtual async Task CompleteJobAsync(long jobID, bool success, string? detailedMessage, CancellationToken cancellationToken)
        {
            string statusCode = success ? "SUC" : "ERR";
            var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            var parms = new[]
            {
                db.GetInt64Parameter("@CompleteDateUTC", DateTime.UtcNow),
                db.GetStringParameter("@StatusCode", statusCode, isFixed: true, size: 3),
                db.GetStringParameter("@DetailedMessage", detailedMessage, isFixed: false, size: -1),
                db.GetInt64Parameter("@JobID", jobID)
            };
            await db.NonQueryAsync(@"
                UPDATE [Jobs]
                SET StatusCode = @StatusCode, DetailedMessage = @DetailedMessage, CompleteDateUTC = @CompleteDateUTC
                WHERE JobID = @JobID;
            ", parms, cancellationToken).ConfigureAwait(false);

            if (statusCode == "SUC")
            {
                var workerManager = ServiceProvider.GetRequiredService<IWorkerManager>();
                var childWorkerIDs = await workerManager.GetChildWorkerIDsByJobAsync(jobID, cancellationToken).ConfigureAwait(false);
                foreach (int childWorkerID in childWorkerIDs)
                {
                    await workerManager.RunNowAsync(childWorkerID, cancellationToken).ConfigureAwait(false);
                }
                // TODO: Email on success?
            }
        }

        public Job ConvertToJob(JobEntity entity)
            => new Job(entity.JobID, entity.ScheduleID,
            DateTime.ParseExact(entity.InsertDateUTC.ToString(), "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture.DateTimeFormat),
            DateTime.ParseExact(entity.QueueDateUTC.ToString(), "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture.DateTimeFormat),
            entity.CompleteDateUTC.HasValue
                ? DateTime.ParseExact(entity.CompleteDateUTC.Value.ToString(), "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture.DateTimeFormat)
                : (DateTime?)null,
            entity.StatusCode, entity.DetailedMessage, entity.AcknowledgementID,
            entity.AcknowledgementDate.HasValue
                ? DateTime.ParseExact(entity.AcknowledgementDate.Value.ToString(), "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture.DateTimeFormat)
                : (DateTime?)null);

        protected abstract string DequeueQuery { get; }
        public virtual async Task<ImmutableArray<JobDetail>> DequeueScheduledJobsAsync(CancellationToken cancellationToken)
        {
            var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            var parms = new[] { db.GetInt64Parameter("@Now", DateTime.UtcNow) };
            var dequeuedJobs = await db.GetManyAsync<JobEntity>(DequeueQuery, 
                parms, Mapper.MapJob, cancellationToken).ConfigureAwait(false);

            if (!dequeuedJobs.Any())
            {
                return ImmutableArray<JobDetail>.Empty;
            }

            return await GetJobDetailsAsync(dequeuedJobs.Select(ConvertToJob), cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<Job> GetJobAsync(long jobID, CancellationToken cancellationToken)
        {
            var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            var parms = new[] { db.GetInt64Parameter("@JobID", jobID) };
            var entity = await db.GetOneAsync<JobEntity>(@"
                SELECT * FROM [Jobs] WHERE JobID = @JobID;
            ", parms, Mapper.MapJob, cancellationToken).ConfigureAwait(false);
            return ConvertToJob(entity);
        }

        public virtual async Task<string?> GetJobDetailedMessageAsync(long jobID, CancellationToken cancellationToken)
        {
            var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            var parms = new[] { db.GetInt64Parameter("@JobID", jobID) };
            return await db.ScalarAsync<string>(@"
                SELECT DetailedMessage FROM Jobs WHERE JobID = @JobID;
            ", parms, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<Job?> GetLastQueuedJobAsync(long scheduleID, CancellationToken cancellationToken)
        {
            var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            var parms = new[]
            {
                db.GetInt64Parameter("@ScheduleID", scheduleID)
            };
            var jobs = await db.GetManyAsync<JobEntity>($@"
                SELECT * FROM Jobs
                WHERE ScheduleID = @ScheduleID
                ORDER BY QueueDateUTC DESC
                {db.GetOffsetLimitClause(0, 1)};
            ", parms, Mapper.MapJob, cancellationToken).ConfigureAwait(false);
            return jobs.Select(ConvertToJob).FirstOrDefault();
        }

        public virtual async Task<ImmutableArray<JobDetail>> GetLatestJobsAsync(int pageNumber,
            int rowsPerPage, string? statusCode, long? workerID, bool overdueOnly,
            CancellationToken cancellationToken)
        {
            var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            var parms = new List<DbParameter>();

            var sql = new StringBuilder();
            sql.AppendLine("SELECT * FROM Jobs WHERE 1=1");
            if (statusCode != null)
            {
                sql.AppendLine("AND StatusCode = @StatusCode");
                parms.Add(db.GetStringParameter("@StatusCode", statusCode, isFixed: true, size: 3));
            }
            if (workerID.HasValue)
            {
                sql.AppendLine("AND ScheduleID IN (SELECT ScheduleID FROM Schedules WHERE WorkerID = @WorkerID)");
                parms.Add(db.GetInt64Parameter("@WorkerID", workerID.Value));
            }
            if (overdueOnly)
            {
                sql.AppendLine("AND StatusCode IN ('ERR', 'NEW', 'RUN')");
            }
            sql.AppendLine("ORDER BY QueueDateUTC DESC");
            sql.AppendLine(db.GetOffsetLimitClause((pageNumber - 1) * rowsPerPage, rowsPerPage));
            sql.Append(";");

            var entities = await db.GetManyAsync<JobEntity>(sql.ToString(), parms,
                Mapper.MapJob, cancellationToken).ConfigureAwait(false);

            var jobs = entities.Select(ConvertToJob).ToImmutableArray();

            IList<Job> filteredJobs;
            if (overdueOnly)
            {
                // For overdue only, filter RUN/ERR/NEW status, and different levels based on the status
                filteredJobs = new List<Job>();
                foreach (var job in jobs)
                {
                    switch (job.StatusCode)
                    {
                        case "RUN": // Greater than the timeout period for the worker
                            var jobDetail = (await GetJobDetailsAsync(new[] { job }, cancellationToken).ConfigureAwait(false))[0];
                            if (job.QueueDateUTC.AddMinutes(jobDetail.Worker.TimeoutMinutes) < DateTime.UtcNow)
                            {
                                filteredJobs.Add(job);
                            }
                            break;
                        case "ERR": // All errors show up here
                            filteredJobs.Add(job);
                            break;
                        case "NEW": // Been stuck in NEW for more than one minute
                            if (job.QueueDateUTC.AddMinutes(1) < DateTime.UtcNow)
                            {
                                filteredJobs.Add(job);
                            }
                            break;
                    }
                }
            }
            else
            {
                filteredJobs = jobs;
            }

            return await GetJobDetailsAsync(filteredJobs, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<ImmutableArray<JobDetail>> GetOverdueJobsAsync(CancellationToken cancellationToken)
            => (await GetLatestJobsAsync(pageNumber: 1, rowsPerPage: 999,
                statusCode: null, workerID: null, overdueOnly: true, cancellationToken).ConfigureAwait(false))
                .OrderBy(x => x.Job.QueueDateUTC)
                .ToImmutableArray();

        public virtual async Task RestartStuckJobsAsync(CancellationToken cancellationToken)
        {
            var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            await db.NonQueryAsync(@"
                UPDATE Jobs SET StatusCode = 'NEW' WHERE StatusCode = 'RUN';
            ", Array.Empty<DbParameter>(), cancellationToken).ConfigureAwait(false);
        }

        private async Task<ImmutableArray<JobDetail>> GetJobDetailsAsync(IEnumerable<Job> jobs, CancellationToken cancellationToken)
        {
            var scheduleManager = ServiceProvider.GetRequiredService<IScheduleManager>();
            var workerManager = ServiceProvider.GetRequiredService<IWorkerManager>();
            var allSchedules = await scheduleManager.GetAllSchedulesAsync(cancellationToken).ConfigureAwait(false);
            var allWorkers = await workerManager.GetAllWorkersAsync(cancellationToken).ConfigureAwait(false);

            var result = new List<JobDetail>();
            foreach (var job in jobs)
            {
                var schedule = allSchedules.SingleOrDefault(s => s.Schedule.ScheduleID == job.ScheduleID)?.Schedule
                    ?? (await scheduleManager.GetScheduleAsync(job.ScheduleID, cancellationToken).ConfigureAwait(false)).Schedule;
                var worker = allWorkers.SingleOrDefault(w => w.WorkerID == schedule.WorkerID)
                    ?? await workerManager.GetWorkerAsync(schedule.WorkerID, cancellationToken).ConfigureAwait(false);
                result.Add(new(job, schedule, worker));
            }
            return result.ToImmutableArray();
        }
    }
}