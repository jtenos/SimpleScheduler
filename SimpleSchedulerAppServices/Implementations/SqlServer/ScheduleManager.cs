using Dapper;
using SimpleSchedulerData;
using SimpleSchedulerDataEntities;

namespace SimpleSchedulerAppServices.Implementations.SqlServer;

/// <summary>
/// SQL Server schedule manager. Data access is via stored procedures in the [app] schema.
/// </summary>
public sealed class ScheduleManager : ScheduleManagerBase
{
    public ScheduleManager(IDatabase db)
        : base(db)
    {
    }

    protected override async Task InsertScheduleCoreAsync(long workerID,
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

        await Db.NonQueryAsync("[app].[Schedules_Insert]", param).ConfigureAwait(false);
    }

    protected override async Task UpdateScheduleCoreAsync(long id,
        bool sunday, bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
        TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC, TimeSpan? recurBetweenEndUTC)
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

        await Db.NonQueryAsync("[app].[Schedules_Update]", param).ConfigureAwait(false);
    }

    protected override async Task DeactivateScheduleCoreAsync(long id)
    {
        DynamicParameters param = new DynamicParameters().AddLongParam("@ID", id);
        await Db.NonQueryAsync("[app].[Schedules_Deactivate]", param).ConfigureAwait(false);
    }

    protected override async Task ReactivateScheduleCoreAsync(long id)
    {
        DynamicParameters param = new DynamicParameters().AddLongParam("@ID", id);
        await Db.NonQueryAsync("[app].[Schedules_Reactivate]", param).ConfigureAwait(false);
    }

    protected override async Task<ScheduleEntity[]> SelectAllCoreAsync(bool includeInactive)
    {
        DynamicParameters param = new DynamicParameters().AddBitParam("@IncludeInactive", includeInactive);
        return await Db.GetManyAsync<ScheduleEntity>("[app].[Schedules_SelectAll]", param).ConfigureAwait(false);
    }

    protected override async Task<ScheduleEntity[]> SelectForWorkerCoreAsync(long workerID)
    {
        DynamicParameters param = new DynamicParameters().AddLongParam("@WorkerID", workerID);
        return await Db.GetManyAsync<ScheduleEntity>("[app].[Schedules_SelectForWorker]", param).ConfigureAwait(false);
    }

    protected override async Task<ScheduleEntity[]> SelectManyCoreAsync(long[] ids)
    {
        DynamicParameters param = new DynamicParameters().AddBigIntArrayParam("@IDs", ids);
        return await Db.GetManyAsync<ScheduleEntity>("[app].[Schedules_SelectMany]", param).ConfigureAwait(false);
    }

    protected override async Task<ScheduleEntity> SelectCoreAsync(long id)
    {
        DynamicParameters param = new DynamicParameters().AddLongParam("@ID", id);
        return await Db.GetOneAsync<ScheduleEntity>("[app].[Schedules_Select]", param).ConfigureAwait(false);
    }
}
