using System.Collections.Immutable;
using Dapper;
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
            cancellationToken
        ).ConfigureAwait(false);
    }

    async Task IScheduleManager.DeactivateScheduleAsync(long id, CancellationToken cancellationToken)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", id);

        await _db.NonQueryAsync(
            "[Schedules_Deactivate]",
            param,
            cancellationToken
        ).ConfigureAwait(false);
    }

    async Task<ImmutableArray<Schedule>> IScheduleManager.GetAllSchedulesAsync(CancellationToken cancellationToken)
    {
        return await _db.GetManyAsync<Schedule>(
            "[app].[Schedules_SelectAll]",
            parameters: null,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);
    }

    async Task<Schedule> IScheduleManager.GetScheduleAsync(long id, CancellationToken cancellationToken)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", id);

        return await _db.GetOneAsync<Schedule>(
                "[app].[Schedules_Select]",
                param,
                cancellationToken
        ).ConfigureAwait(false);
    }

    async Task<ImmutableArray<Schedule>> IScheduleManager.GetSchedulesToInsertAsync(CancellationToken cancellationToken)
    {
        return await _db.GetManyAsync<Schedule>(
                "[app].[Schedules_SelectForJobInsertion]",
                parameters: null,
                cancellationToken
        ).ConfigureAwait(false);
    }

    async Task IScheduleManager.ReactivateScheduleAsync(long id, CancellationToken cancellationToken)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", id);

        await _db.NonQueryAsync(
            "[app].[Schedules_Reactivate]",
            param,
            cancellationToken
        ).ConfigureAwait(false);
    }

    async Task IScheduleManager.UpdateScheduleAsync(long id, bool sunday,
        bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
        TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC,
        TimeSpan? recurBetweenEndUTC, CancellationToken cancellationToken)
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
            param,
            cancellationToken
        ).ConfigureAwait(false);
    }
}
