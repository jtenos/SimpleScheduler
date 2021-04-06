using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleSchedulerData;
using SimpleSchedulerEntities;
using SimpleSchedulerModels;
using SimpleSchedulerModels.Exceptions;

namespace SimpleSchedulerBusiness
{
    public abstract class WorkerManagerBase
        : IWorkerManager
    {
        protected WorkerManagerBase(DatabaseFactory databaseFactory, IServiceProvider serviceProvider)
            => (DatabaseFactory, ServiceProvider) = (databaseFactory, serviceProvider);

        protected DatabaseFactory DatabaseFactory { get; }
        protected IServiceProvider ServiceProvider { get; }

        public virtual async Task RunNowAsync(long workerID, CancellationToken cancellationToken)
        {
            long scheduleID = await ServiceProvider.GetRequiredService<IScheduleManager>()
                .AddScheduleAsync(
                    workerID,
                    isActive: false,
                    sunday: true,
                    monday: true,
                    tuesday: true,
                    wednesday: true,
                    thursday: true,
                    friday: true,
                    saturday: true,
                    timeOfDayUTC: TimeSpan.Zero,
                    recurTime: null,
                    recurBetweenStartUTC: null,
                    recurBetweenEndUTC: null,
                    oneTime: true,
                    cancellationToken
                ).ConfigureAwait(false);

            await ServiceProvider.GetRequiredService<IJobManager>()
                .AddJobAsync(
                    scheduleID,
                    queueDateUTC: DateTime.UtcNow,
                    cancellationToken
            ).ConfigureAwait(false);
        }

        public virtual async Task<ImmutableArray<long>> GetChildWorkerIDsByJobAsync(long jobID, CancellationToken cancellationToken)
        {
            var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            var parameters = new[] {
                db.GetInt64Parameter("@JobID", jobID)
            };
            Func<DbDataReader, long> mapFunc = rdr => Convert.ToInt64(rdr[0]);
            const string sql = @"
                ;WITH parent AS (
                    SELECT s.WorkerID
                    FROM Jobs j
                    JOIN Schedules s ON j.ScheduleID = s.ScheduleID
                    JOIN Workers w ON s.WorkerID = w.WorkerID
                    WHERE j.JobID = @JobID
                    AND s.IsActive = 1
                    AND w.IsActive = 1
                )
                SELECT child.WorkerID
                FROM Workers child
                JOIN parent ON child.ParentWorkerID = parent.WorkerID
                WHERE child.IsActive = 1;
            ";
            var containers = await db.GetManyAsync<long>(sql,
                parameters, mapFunc, cancellationToken).ConfigureAwait(false);
            return containers.ToImmutableArray();
        }

        public virtual async Task<ImmutableArray<WorkerDetail>> GetAllWorkerDetailsAsync(CancellationToken cancellationToken,
            bool getActive, bool getInactive)
            => await GetWorkerDetailsAsync(
                await GetAllWorkersAsync(cancellationToken, getActive, getInactive).ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);

        public virtual async Task<ImmutableArray<Worker>> GetAllWorkersAsync(CancellationToken cancellationToken,
            bool getActive, bool getInactive)
        {
            if (!getActive && !getInactive) { return ImmutableArray<Worker>.Empty; }
            var sql = new StringBuilder("SELECT * FROM Workers");
            if (getActive && !getInactive) sql.Append(" WHERE IsActive = 1;");
            else if (!getActive && getInactive) sql.Append(" WHERE IsActive = 0;");
            else { sql.Append(";"); }
            var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            var entities = await db.GetManyAsync<WorkerEntity>(sql.ToString(), Array.Empty<DbParameter>(),
                Mapper.MapWorker, cancellationToken).ConfigureAwait(false);

            return entities.Select(ConvertToWorker).ToImmutableArray();
        }

        public virtual async Task<Worker> GetWorkerAsync(long workerID, CancellationToken cancellationToken)
        {
            var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            var parms = new[]
            {
                db.GetInt64Parameter("@WorkerID", workerID)
            };
            var entity = await db.GetOneAsync<WorkerEntity>(@"
                SELECT * FROM Workers WHERE WorkerID = @WorkerID;",
                parms, Mapper.MapWorker, cancellationToken).ConfigureAwait(false);

            return ConvertToWorker(entity);
        }

        public virtual async Task<long> AddWorkerAsync(bool isActive, string workerName,
            string detailedDescription, string emailOnSuccess, long? parentWorkerID, long timeoutMinutes,
            string directoryName, string executable, string argumentValues, CancellationToken cancellationToken)
        {
            var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            var parms = new[]
            {
                db.GetStringParameter("@WorkerName", workerName, isFixed: false, size: 100)
            };
            var existingWorkers = await db.GetManyAsync<WorkerEntity>(@"
                SELECT * FROM Workers WHERE WorkerName = @WorkerName;
                ", parms, Mapper.MapWorker, cancellationToken).ConfigureAwait(false);

            if (existingWorkers.Any())
            {
                throw new WorkerAlreadyExistsException(workerName);
            }

            parms = new[]
            {
                db.GetInt64Parameter("@IsActive", isActive),
                db.GetStringParameter("@WorkerName", workerName, isFixed: false, size: 100),
                db.GetStringParameter("@DetailedDescription", detailedDescription, isFixed: false, size: -1),
                db.GetStringParameter("@EmailOnSuccess", emailOnSuccess, isFixed: false, size: 100),
                db.GetInt64Parameter("@ParentWorkerID", parentWorkerID),
                db.GetInt64Parameter("@TimeoutMinutes", timeoutMinutes),
                db.GetStringParameter("@DirectoryName", directoryName, isFixed: false, size: 1000),
                db.GetStringParameter("@Executable", executable, isFixed: false, size: 1000),
                db.GetStringParameter("@ArgumentValues", argumentValues, isFixed: false, size: 1000)
            };

            long workerID = await db.ScalarAsync<long>($@"
                INSERT INTO Workers (
                    IsActive, WorkerName, DetailedDescription, EmailOnSuccess, ParentWorkerID, TimeoutMinutes
                    , DirectoryName, [Executable], ArgumentValues
                )
                VALUES (
                    @IsActive, @WorkerName, @DetailedDescription, @EmailOnSuccess, @ParentWorkerID, @TimeoutMinutes
                    , @DirectoryName, @Executable, @ArgumentValues
                );
                {db.GetLastAutoIncrementQuery}
            ", parms, cancellationToken).ConfigureAwait(false);

            await EnsureNoCircularWorkersAsync(workerID, cancellationToken).ConfigureAwait(false);
            return workerID;
        }

        public virtual async Task UpdateWorkerAsync(long workerID, bool isActive, string workerName,
            string detailedDescription, string emailOnSuccess, long? parentWorkerID, long timeoutMinutes,
            string directoryName, string executable, string argumentValues, CancellationToken cancellationToken)
        {
            var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            DbParameter[] parms =
            {
                db.GetInt64Parameter("@WorkerID", workerID),
                db.GetInt64Parameter("@IsActive", isActive),
                db.GetStringParameter("@WorkerName", workerName, isFixed: false, size: 100),
                db.GetStringParameter("@DetailedDescription", detailedDescription, isFixed: false, size: -1),
                db.GetStringParameter("@EmailOnSuccess", emailOnSuccess, isFixed: false, size: 100),
                db.GetInt64Parameter("@ParentWorkerID", parentWorkerID),
                db.GetInt64Parameter("@TimeoutMinutes", timeoutMinutes),
                db.GetStringParameter("@DirectoryName", directoryName, isFixed: false, size: 1000),
                db.GetStringParameter("@Executable", executable, isFixed: false, size: 1000),
                db.GetStringParameter("@ArgumentValues", argumentValues, isFixed: false, size: 1000)
            };
            await db.NonQueryAsync(@"
                UPDATE Workers
                SET
                    IsActive = @IsActive
                    ,WorkerName = @WorkerName
                    ,DetailedDescription = @DetailedDescription
                    ,EmailOnSuccess = @EmailOnSuccess
                    ,ParentWorkerID = @ParentWorkerID
                    ,TimeoutMinutes = @TimeoutMinutes
                    ,DirectoryName = @DirectoryName
                    ,[Executable] = @Executable
                    ,ArgumentValues = @ArgumentValues
                WHERE WorkerID = @WorkerID;
                ", parms, cancellationToken).ConfigureAwait(false);

            await EnsureNoCircularWorkersAsync(workerID, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task DeactivateWorkerAsync(long workerID, CancellationToken cancellationToken)
        {
            var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);

            var worker = await GetWorkerAsync(workerID, cancellationToken).ConfigureAwait(false);
            if (!worker.IsActive) { return; }

            string newName = $"INACTIVE: {DateTime.UtcNow:yyyyMMddHHmmss} {worker.WorkerName}".Trim();
            if (newName.Length > 100) { newName = newName.Substring(0, 100); }
            DbParameter[] parms =
            {
                   db.GetInt64Parameter("@WorkerID", workerID),
                   db.GetStringParameter("@WorkerName", newName, isFixed: false, size: 100)
            };
            await db.NonQueryAsync(@"
                UPDATE Workers
                SET
                    IsActive = 0
                    ,WorkerName = @WorkerName
                WHERE WorkerID = @WorkerID;
            ", parms, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task ReactivateWorkerAsync(long workerID, CancellationToken cancellationToken)
        {
            var worker = await GetWorkerAsync(workerID, cancellationToken).ConfigureAwait(false);
            if (worker.IsActive) { return; }

            if (Regex.IsMatch(worker.WorkerName, @"^INACTIVE\: [0-9]{14}.*$"))
            {
                string workerName = $"{worker.WorkerName[24..]} (react {DateTime.UtcNow:yyyyMMddHHmmss})".Trim();
                await UpdateWorkerAsync(worker.WorkerID, isActive: true, workerName, worker.DetailedDescription,
                    worker.EmailOnSuccess, worker.ParentWorkerID, worker.TimeoutMinutes, 
                    worker.DirectoryName, worker.Executable, worker.ArgumentValues, cancellationToken).ConfigureAwait(false);
            }
        }

        public Worker ConvertToWorker(WorkerEntity entity)

            => new Worker(
                entity.WorkerID,
                entity.IsActive == 1,
                entity.WorkerName, entity.DetailedDescription,
                entity.EmailOnSuccess, entity.ParentWorkerID, entity.TimeoutMinutes,
                entity.DirectoryName, entity.Executable,
                entity.ArgumentValues
            );

        private async Task EnsureNoCircularWorkersAsync(long workerID, CancellationToken cancellationToken)
        {
            var allWorkers = await GetAllWorkersAsync(cancellationToken, getActive: true, getInactive: true);

            long? GetParentWorkerID(long workerID) => allWorkers.Single(x => x.WorkerID == workerID).ParentWorkerID;

            var descendantWorkerIDs = new HashSet<long>();
            descendantWorkerIDs.Add(workerID);

            long? parentWorkerID = GetParentWorkerID(workerID);
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
            var allSchedules = await ServiceProvider.GetRequiredService<IScheduleManager>().GetAllSchedulesAsync(
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
