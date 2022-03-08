using Dapper;
using OneOf;
using OneOf.Types;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerData;
using SimpleSchedulerDataEntities;
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

    async Task<OneOf<Success, Error<string>>> IScheduleManager.AddScheduleAsync(long workerID,
        bool sunday, bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
        TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC, TimeSpan? recurBetweenEndUTC)
    {
        if (!sunday && !monday && !tuesday && !wednesday && !thursday && !friday && !saturday)
        {
            return new Error<string>("You must select at least one day.");
        }
        if (timeOfDayUTC.HasValue && recurTime.HasValue)
        {
            return new Error<string>("You must select only one of TimeOfDay/RecurTime");
        }
        if (!timeOfDayUTC.HasValue && !recurTime.HasValue)
        {
            return new Error<string>("You must select one of TimeOfDay/RecurTime");
        }
        if (recurBetweenStartUTC.HasValue && recurBetweenEndUTC.HasValue && recurBetweenStartUTC > recurBetweenEndUTC)
        {
            return new Error<string>("Recur between times invalid");
        }

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
            param
        ).ConfigureAwait(false);

        return new Success();
    }

    async Task IScheduleManager.DeactivateScheduleAsync(long id)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", id);

        await _db.NonQueryAsync(
            "[app].[Schedules_Deactivate]",
            param
        ).ConfigureAwait(false);
    }

    async Task<Schedule[]> IScheduleManager.GetAllSchedulesAsync()
    {
        return (await _db.GetManyAsync<ScheduleEntity>(
            "[app].[Schedules_SelectAll]",
            parameters: null
        ).ConfigureAwait(false))
        .Select(s => ModelBuilders.GetSchedule(s))
        .ToArray();
    }

    async Task<Schedule[]> IScheduleManager.GetSchedulesForWorkerAsync(long workerID)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@WorkerID", workerID);

        return (await _db.GetManyAsync<ScheduleEntity>(
            "[app].[Schedules_SelectForWorker]",
            parameters: param
        ).ConfigureAwait(false))
        .Select(s => ModelBuilders.GetSchedule(s))
        .ToArray();
    }

    async Task<Schedule> IScheduleManager.GetScheduleAsync(long id)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", id);

        return ModelBuilders.GetSchedule(await _db.GetOneAsync<ScheduleEntity>(
                "[app].[Schedules_Select]",
                param
        ).ConfigureAwait(false));
    }

    async Task<Schedule[]> IScheduleManager.GetSchedulesToInsertAsync()
    {
        return (await _db.GetManyAsync<ScheduleEntity>(
                "[app].[Schedules_SelectForJobInsertion]",
                parameters: null
        ).ConfigureAwait(false))
        .Select(s => ModelBuilders.GetSchedule(s))
        .ToArray();
    }

    async Task IScheduleManager.ReactivateScheduleAsync(long id)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", id);

        await _db.NonQueryAsync(
            "[app].[Schedules_Reactivate]",
            param
        ).ConfigureAwait(false);
    }

    async Task<OneOf<Success, Error<string>>> IScheduleManager.UpdateScheduleAsync(long id, bool sunday,
        bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
        TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC,
        TimeSpan? recurBetweenEndUTC)
    {
        if (!sunday && !monday && !tuesday && !wednesday && !thursday && !friday && !saturday)
        {
            return new Error<string>("You must select at least one day.");
        }
        if (timeOfDayUTC.HasValue && recurTime.HasValue)
        {
            return new Error<string>("You must select only one of TimeOfDay/RecurTime");
        }
        if (!timeOfDayUTC.HasValue && !recurTime.HasValue)
        {
            return new Error<string>("You must select one of TimeOfDay/RecurTime");
        }
        if (recurBetweenStartUTC.HasValue && recurBetweenEndUTC.HasValue && recurBetweenStartUTC > recurBetweenEndUTC)
        {
            return new Error<string>("Recur between times invalid");
        }

        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", id)
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
            "[app].[Schedules_Update]",
            param
        ).ConfigureAwait(false);

        return new Success();
    }
}
