using System.Collections.Immutable;
using System.Data.Common;
using System.Globalization;
using System.Text;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerData;
using SimpleSchedulerModels;

namespace SimpleSchedulerAppServices.Implementations;

public sealed class ScheduleManager
    : IScheduleManager
{
    private readonly SqlDatabase _db;

    public ScheduleManager(SqlDatabase db)
    {
        _db = db;
    }

    async Task IScheduleManager.AddScheduleAsync(long workerID,
        bool sunday, bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
        TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC, TimeSpan? recurBetweenEndUTC,
        CancellationToken cancellationToken)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@WorkerID", workerID)
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
            .AddNullableTimeParam("@RecurBetweenEndUTC", recurBetweenEndUTC);

        await _db.NonQueryAsync(
            "[app].[Schedules_Insert]",
            param,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);
    }

    async Task IScheduleManager.DeactivateScheduleAsync(long scheduleID, CancellationToken cancellationToken)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", scheduleID);

        await _db.NonQueryAsync(
            "[Schedules_Deactivate]",
            param,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);
    }

    async Task<ImmutableArray<ScheduleDetail>> IScheduleManager.GetAllSchedulesAsync(
        bool getActive, bool getInactive, bool getOneTime, CancellationToken cancellationToken)
    {
        if (!getActive && !getInactive) { return ImmutableArray<ScheduleDetail>.Empty; }

        DynamicParameters param = new DynamicParameters()
            .AddBitParam("@GetActive", getActive)
            .AddBitParam("@GetInactive", getInactive)
            .AddBitParam("@GetOneTime", getOneTime);

        (ImmutableArray<Schedule> schedules, ImmutableArray<Worker> workers)
            = await _db.GetManyAsync<Schedule, Worker>(
           "[app].[Schedules_Search]",
           param,
           cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        return schedules.Select(s => new ScheduleDetail(
            s,
            workers.Single(w => w.ID == s.WorkerID)
        )).ToImmutableArray();
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

    public virtual async Task UpdateScheduleAsync(long scheduleID, bool sunday,
        bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
        TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC,
        TimeSpan? recurBetweenEndUTC, CancellationToken cancellationToken)
    {
        var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        var parms = new[]
        {
                db.GetInt64Parameter("@ScheduleID", scheduleID),
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
                    Sunday = @Sunday
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
