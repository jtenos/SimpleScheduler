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

namespace SimpleSchedulerBusiness
{
    public abstract class ScheduleManagerBase
        : IScheduleManager
    {
        protected ScheduleManagerBase(DatabaseFactory databaseFactory, IServiceProvider serviceProvider)
            => (DatabaseFactory, ServiceProvider) = (databaseFactory, serviceProvider);

        protected DatabaseFactory DatabaseFactory { get; }
        protected IServiceProvider ServiceProvider { get; }

        public virtual async Task<long> AddScheduleAsync(long workerID,
            bool isActive, bool sunday, bool monday, bool tuesday, bool wednesday,
            bool thursday, bool friday, bool saturday, TimeSpan? timeOfDayUTC,
            TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC, TimeSpan? recurBetweenEndUTC,
            bool oneTime, CancellationToken cancellationToken)
        {
            var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            var parms = new[]
            {
                db.GetInt64Parameter("@WorkerID", workerID),
                db.GetInt64Parameter("@IsActive", isActive),
                db.GetInt64Parameter("@Sunday", sunday),
                db.GetInt64Parameter("@Monday", monday),
                db.GetInt64Parameter("@Tuesday", tuesday),
                db.GetInt64Parameter("@Wednesday", wednesday),
                db.GetInt64Parameter("@Thursday", thursday),
                db.GetInt64Parameter("@Friday", friday),
                db.GetInt64Parameter("@Saturday", saturday),
                db.GetInt64Parameter("@TimeOfDayUTC", timeOfDayUTC),
                db.GetInt64Parameter("@RecurTime", recurTime),
                db.GetInt64Parameter("@RecurBetweenStartUTC", recurBetweenStartUTC),
                db.GetInt64Parameter("@RecurBetweenEndUTC", recurBetweenEndUTC),
                db.GetInt64Parameter("@OneTime", oneTime)
            };
            return await db.ScalarAsync<long>($@"
                INSERT INTO [Schedules] (
                    WorkerID, IsActive, Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday
                    ,TimeOfDayUTC, RecurTime, RecurBetweenStartUTC, RecurBetweenEndUTC, OneTime
                )
                VALUES (
                    @WorkerID, @IsActive, @Sunday, @Monday, @Tuesday, @Wednesday, @Thursday, @Friday, @Saturday
                    ,@TimeOfDayUTC, @RecurTime, @RecurBetweenStartUTC, @RecurBetweenEndUTC, @OneTime
                );

                {db.GetLastAutoIncrementQuery}
            ", parms, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task DeactivateScheduleAsync(long scheduleID, CancellationToken cancellationToken)
        {
            var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            var parms = new[]
            {
                db.GetInt64Parameter("@ScheduleID", scheduleID)
            };
            await db.NonQueryAsync(@"
                UPDATE [Schedules]
                SET IsActive = 0
                WHERE ScheduleID = @ScheduleID;

                DELETE FROM [Jobs] WHERE ScheduleID = @ScheduleID AND StatusCode = 'NEW';
            ", parms, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<ImmutableArray<ScheduleDetail>> GetAllSchedulesAsync(
            CancellationToken cancellationToken, bool getActive = true,
            bool getInactive = false, bool getOneTime = false)
        {
            if (!getActive && !getInactive) { return ImmutableArray<ScheduleDetail>.Empty; }

            var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            var sql = new StringBuilder();
            sql.AppendLine("SELECT * FROM Schedules WHERE 1=1");
            if (!getOneTime) { sql.Append(" AND OneTime = 0 "); }
            if (getActive && !getInactive) { sql.Append(" AND IsActive = 1;"); }
            else if (!getActive && getInactive) { sql.Append(" AND IsActive = 0;"); }
            else { sql.Append(";"); }
            var allScheduleEntities = await db.GetManyAsync<ScheduleEntity>(sql.ToString(),
                Array.Empty<DbParameter>(), Mapper.MapSchedule, cancellationToken).ConfigureAwait(false);
            var allSchedules = allScheduleEntities.Select(ConvertToSchedule).ToImmutableArray();
            return await GetScheduleDetailsAsync(allSchedules, cancellationToken).ConfigureAwait(false); ;
        }

        public virtual async Task<ScheduleDetail> GetScheduleAsync(long scheduleID, CancellationToken cancellationToken)
        {
            var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            var parms = new[] { db.GetInt64Parameter("@ScheduleID", scheduleID) };
            var schedule = await db.GetOneAsync<ScheduleEntity>(@"
                    SELECT * FROM [Schedules] WHERE ScheduleID = @ScheduleID;
                ", parms, Mapper.MapSchedule, cancellationToken).ConfigureAwait(false);

            var scheduleDetails = await GetScheduleDetailsAsync(new[] { ConvertToSchedule(schedule) }, cancellationToken).ConfigureAwait(false);
            return scheduleDetails.Single();
        }

        public virtual async Task<ImmutableArray<ScheduleDetail>> GetSchedulesToInsertAsync(CancellationToken cancellationToken)
        {
            var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            var schedulesToInsert = await db.GetManyAsync<ScheduleEntity>(@"
                SELECT * FROM Schedules
                WHERE IsActive = 1
                AND ScheduleID NOT IN (
                    SELECT ScheduleID FROM Jobs WHERE StatusCode IN ('NEW', 'RUN')
                );
            ", Array.Empty<DbParameter>(), Mapper.MapSchedule, cancellationToken);

            var scheduleDetails = await GetScheduleDetailsAsync(schedulesToInsert.Select(ConvertToSchedule),
                cancellationToken).ConfigureAwait(false);
            return scheduleDetails.ToImmutableArray();
        }

        public virtual async Task ReactivateScheduleAsync(long scheduleID, CancellationToken cancellationToken)
        {
            var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            var parms = new[]
            {
                db.GetInt64Parameter("@ScheduleID", scheduleID)
            };
            await db.NonQueryAsync(@"
                UPDATE Schedules
                SET IsActive = 1
                WHERE ScheduleID = @ScheduleID;
            ", parms, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task UpdateScheduleAsync(long scheduleID, long workerID, bool sunday,
            bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
            TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC,
            TimeSpan? recurBetweenEndUTC, CancellationToken cancellationToken)
        {
            var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            var parms = new[]
            {
                db.GetInt64Parameter("@ScheduleID", scheduleID),
                db.GetInt64Parameter("@WorkerID", workerID),
                db.GetInt64Parameter("@Sunday", sunday),
                db.GetInt64Parameter("@Monday", monday),
                db.GetInt64Parameter("@Tuesday", tuesday),
                db.GetInt64Parameter("@Wednesday", wednesday),
                db.GetInt64Parameter("@Thursday", thursday),
                db.GetInt64Parameter("@Friday", friday),
                db.GetInt64Parameter("@Saturday", saturday),
                db.GetInt64Parameter("@TimeOfDayUTC", timeOfDayUTC),
                db.GetInt64Parameter("@RecurTime", recurTime),
                db.GetInt64Parameter("@RecurBetweenStartUTC", recurBetweenStartUTC),
                db.GetInt64Parameter("@RecurBetweenEndUTC", recurBetweenEndUTC)
            };

            await db.NonQueryAsync(@"
                UPDATE Schedules SET
                    WorkerID = @WorkerID
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
                DELETE FROM Jobs WHERE ScheduleID = @ScheduleID AND StatusCode = 'NEW';

            ", parms, cancellationToken).ConfigureAwait(false);
        }

        public Schedule ConvertToSchedule(ScheduleEntity entity)
            => new Schedule(entity.ScheduleID,
                entity.IsActive == 1,
                entity.WorkerID,
                entity.Sunday == 1, entity.Monday == 1, entity.Tuesday == 1, entity.Wednesday == 1,
                entity.Thursday == 1, entity.Friday == 1, entity.Saturday == 1,
                ConvertToSimpleTimeSpan(entity.TimeOfDayUTC),
                ConvertToSimpleTimeSpan(entity.RecurTime),
                ConvertToSimpleTimeSpan(entity.RecurBetweenStartUTC),
                ConvertToSimpleTimeSpan(entity.RecurBetweenEndUTC),
                entity.OneTime == 1);

        private static SimpleTimeSpan? ConvertToSimpleTimeSpan(long? value)
        {
            if (!value.HasValue) return null;
            var ts = TimeSpan.ParseExact(value.Value.ToString("000000000"), "hhmmssfff",
                CultureInfo.InvariantCulture.DateTimeFormat);
            return new SimpleTimeSpan(ts.Hours, ts.Minutes);
        }

        private async Task<ImmutableArray<ScheduleDetail>> GetScheduleDetailsAsync(IEnumerable<Schedule> schedules,
           CancellationToken cancellationToken)
        {
            var allWorkers = await ServiceProvider.GetRequiredService<IWorkerManager>().GetAllWorkersAsync(
                cancellationToken, getActive: true, getInactive: true).ConfigureAwait(false);

            var result = new List<ScheduleDetail>();
            foreach (var schedule in schedules)
            {
                var worker = allWorkers.Single(w => w.WorkerID == schedule.WorkerID);
                result.Add(new(schedule, worker));
            }
            return result.ToImmutableArray();
        }
    }
}
