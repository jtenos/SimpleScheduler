using Dapper;
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

    async Task IScheduleManager.AddScheduleAsync(long workerID,
        bool sunday, bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
        TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC, TimeSpan? recurBetweenEndUTC)
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
            param
        ).ConfigureAwait(false);
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

    async Task IScheduleManager.UpdateScheduleAsync(long id, bool sunday,
        bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
        TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC,
        TimeSpan? recurBetweenEndUTC)
    {
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
    }
}
