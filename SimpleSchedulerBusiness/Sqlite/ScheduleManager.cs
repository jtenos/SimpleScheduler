using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SimpleSchedulerData;
using SimpleSchedulerModels;

namespace SimpleSchedulerBusiness.Sqlite
{
    public class ScheduleManager
        : BaseManager, IScheduleManager
    {
        public ScheduleManager(IDatabaseFactory databaseFactory, IServiceProvider serviceProvider)
            : base(databaseFactory, serviceProvider) { }

        async Task IScheduleManager.DeactivateScheduleAsync(int scheduleID, CancellationToken cancellationToken)
            => await NonQueryAsync(@"
                UPDATE [Schedules]
                SET IsActive = 0, UpdateDateTime = @Now
                WHERE ScheduleID = @ScheduleID;

                DELETE FROM [Jobs] WHERE ScheduleID = @ScheduleID AND StatusCode = 'NEW';
            ", CreateDynamicParameters()
                .AddIntParam("@ScheduleID", scheduleID)
                .AddDateTime2Param("@Now", DateTime.UtcNow), cancellationToken).ConfigureAwait(false);

        async Task IScheduleManager.ReactivateScheduleAsync(int scheduleID, CancellationToken cancellationToken)
            => await NonQueryAsync(@"
                UPDATE [Schedules]
                SET IsActive = 1, UpdateDateTime = @Now
                WHERE ScheduleID = @ScheduleID;
            ", CreateDynamicParameters()
                .AddIntParam("@ScheduleID", scheduleID)
                .AddDateTime2Param("@Now", DateTime.UtcNow), cancellationToken).ConfigureAwait(false);

        async Task<ImmutableArray<ScheduleDetail>> IScheduleManager.GetSchedulesToInsertAsync(CancellationToken cancellationToken)
        {
            var schedulesToInsert = await GetManyAsync<Schedule>(@"
                SELECT * FROM Schedules s
                WHERE s.IsActive = 1
                AND s.ScheduleID NOT IN (
                    SELECT ScheduleID FROM [Jobs] WHERE StatusCode IN ('NEW', 'RUN')
                );
            ", CreateDynamicParameters(), cancellationToken);

            var scheduleDetails = await GetScheduleDetailsAsync(schedulesToInsert, cancellationToken).ConfigureAwait(false);
            return scheduleDetails.Where(x => x.Worker.IsActive).ToImmutableArray();
        }

        async Task<ImmutableArray<Schedule>> IScheduleManager.GetAllSchedulesAsync(CancellationToken cancellationToken,
            bool getActive, bool getInactive)
        {
            if (!getActive && !getInactive) { return ImmutableArray<Schedule>.Empty; }

            var sql = new StringBuilder();
            sql.AppendLine("SELECT * FROM [Schedules];");
            var allSchedules = await GetManyAsync<Schedule>(sql.ToString(), CreateDynamicParameters(), cancellationToken).ConfigureAwait(false);
            return allSchedules
                .Where(s => (getActive && s.IsActive) || (getInactive && !s.IsActive))
                .ToImmutableArray();
        }

        async Task<ScheduleDetail> IScheduleManager.GetScheduleAsync(int scheduleID, CancellationToken cancellationToken)
        {
            var schedule = await GetOneAsync<Schedule>(@"
                    SELECT * FROM [Schedules] WHERE ScheduleID = @ScheduleID;
                ",
                CreateDynamicParameters()
                .AddIntParam("@ScheduleID", scheduleID),
                cancellationToken).ConfigureAwait(false);

            var scheduleDetails = await GetScheduleDetailsAsync(new[] { schedule }, cancellationToken).ConfigureAwait(false);
            return scheduleDetails.Single();
        }

        async Task<int> IScheduleManager.AddScheduleAsync(int workerID, bool isActive, bool sunday,
            bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
            TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC,
            TimeSpan? recurBetweenEndUTC, bool oneTime, CancellationToken cancellationToken)
            => (await ScalarAsync<int>(@"
                INSERT INTO [Schedules] (
                    WorkerID, IsActive, Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday
                    ,TimeOfDayUTC, RecurTime, RecurBetweenStartUTC, RecurBetweenEndUTC, OneTime
                )
                VALUES (
                    @WorkerID, @IsActive, @Sunday, @Monday, @Tuesday, @Wednesday, @Thursday, @Friday, @Saturday
                    ,@TimeOfDayUTC, @RecurTime, @RecurBetweenStartUTC, @RecurBetweenEndUTC, @OneTime
                );

                SELECT last_insert_rowid();
            ", CreateDynamicParameters()
                .AddIntParam("@WorkerID", workerID)
                .AddBitParam("@IsActive", isActive)
                .AddBitParam("@Sunday", sunday)
                .AddBitParam("@Monday", monday)
                .AddBitParam("@Tuesday", tuesday)
                .AddBitParam("@Wednesday", wednesday)
                .AddBitParam("@Thursday", thursday)
                .AddBitParam("@Friday", friday)
                .AddBitParam("@Saturday", saturday)
                .AddNullableTimeParam("@TimeOfDayUTC", timeOfDayUTC)
                .AddNullableTimeParam("@RecurTime", recurTime)
                .AddNullableTimeParam("@RecurBetweenStartUTC", recurBetweenStartUTC)
                .AddNullableTimeParam("@RecurBetweenEndUTC", recurBetweenEndUTC)
                .AddBitParam("@OneTime", oneTime),
                cancellationToken).ConfigureAwait(false));

        async Task IScheduleManager.UpdateScheduleAsync(int scheduleID, int workerID, bool sunday,
            bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
            TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC,
            TimeSpan? recurBetweenEndUTC, CancellationToken cancellationToken)
            => await NonQueryAsync(@"
            UPDATE [Schedules] SET
                WorkerID = @WorkerID
                ,UpdateDateTime = @Now
                ,Sunday = @Sunday
                ,Monday = @Monday
                ,Tuesday = @Tuesday
                ,Wednesday = @Wednesday
                ,Thursday = @Thursday
                ,Friday = @Friday
                ,Saturday = @Saturday
                ,TimeOfDayUTC = @TimeOfDayUTC
                ,RecurTime = @RecurTime
                ,RecurBetweenStartUTC = @RecurBetweenStartUTC
                ,RecurBetweenEndUTC = @RecurBetweenEndUTC
            WHERE ScheduleID = @ScheduleID;

            -- Clears out the job queue so it will create the next one at the right time
            DELETE [Jobs] WHERE ScheduleID = @ScheduleID AND StatusCode = 'NEW';

            ", CreateDynamicParameters()
                .AddIntParam("@ScheduleID", scheduleID)
                .AddDateTime2Param("@Now", DateTime.UtcNow)
                .AddIntParam("@WorkerID", workerID)
                .AddBitParam("@Sunday", sunday)
                .AddBitParam("@Monday", monday)
                .AddBitParam("@Tuesday", tuesday)
                .AddBitParam("@Wednesday", wednesday)
                .AddBitParam("@Thursday", thursday)
                .AddBitParam("@Friday", friday)
                .AddBitParam("@Saturday", saturday)
                .AddNullableTimeParam("@TimeOfDayUTC", timeOfDayUTC)
                .AddNullableTimeParam("@RecurTime", recurTime)
                .AddNullableTimeParam("@RecurBetweenStartUTC", recurBetweenStartUTC)
                .AddNullableTimeParam("@RecurBetweenEndUTC", recurBetweenEndUTC),
                cancellationToken).ConfigureAwait(false);

        private async Task<ImmutableArray<ScheduleDetail>> GetScheduleDetailsAsync(IEnumerable<Schedule> schedules,
            CancellationToken cancellationToken)
        {
            var allWorkers = await GetWorkerManager().GetAllWorkersAsync(cancellationToken).ConfigureAwait(false);

            var result = new List<ScheduleDetail>();
            foreach (var schedule in schedules)
            {
                var worker = allWorkers.SingleOrDefault(w => w.WorkerID == schedule.WorkerID)
                    ?? await GetWorkerManager().GetWorkerAsync(schedule.WorkerID, cancellationToken).ConfigureAwait(false);
                result.Add(new(schedule, worker));
            }
            return result.ToImmutableArray();
        }
    }
}
