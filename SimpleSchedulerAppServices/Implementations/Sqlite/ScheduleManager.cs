using Dapper;
using SimpleSchedulerData;
using SimpleSchedulerDataEntities;

namespace SimpleSchedulerAppServices.Implementations.Sqlite;

/// <summary>
/// SQLite schedule manager. Data access is via SQL scripts equivalent to the SQL Server procedures.
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
        DynamicParameters param = BuildScheduleParams(sunday, monday, tuesday, wednesday, thursday, friday, saturday,
            timeOfDayUTC, recurTime, recurBetweenStartUTC, recurBetweenEndUTC);
        param.Add("@WorkerID", workerID);

        await Db.NonQueryAsync(@"
            INSERT INTO Schedules (
                WorkerID, Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday,
                TimeOfDayUTC, RecurTime, RecurBetweenStartUTC, RecurBetweenEndUTC, OneTime
            ) VALUES (
                @WorkerID, @Sunday, @Monday, @Tuesday, @Wednesday, @Thursday, @Friday, @Saturday,
                @TimeOfDayUTC, @RecurTime, @RecurBetweenStartUTC, @RecurBetweenEndUTC, 0
            );",
            param
        ).ConfigureAwait(false);
    }

    protected override async Task UpdateScheduleCoreAsync(long id,
        bool sunday, bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
        TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC, TimeSpan? recurBetweenEndUTC)
    {
        DynamicParameters param = BuildScheduleParams(sunday, monday, tuesday, wednesday, thursday, friday, saturday,
            timeOfDayUTC, recurTime, recurBetweenStartUTC, recurBetweenEndUTC);
        param.Add("@ID", id);

        await Db.NonQueryAsync(@"
            BEGIN;

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
            WHERE ID = @ID;

            -- Clears out the job queue so it will create the next one at the right time
            DELETE FROM Jobs WHERE ScheduleID = @ID AND StatusCode = 'NEW';

            COMMIT;",
            param
        ).ConfigureAwait(false);
    }

    protected override async Task DeactivateScheduleCoreAsync(long id)
    {
        DynamicParameters param = new();
        param.Add("@ID", id);

        await Db.NonQueryAsync(@"
            BEGIN;
            UPDATE Schedules SET IsActive = 0 WHERE ID = @ID;
            DELETE FROM Jobs WHERE ScheduleID = @ID AND StatusCode = 'NEW';
            COMMIT;",
            param
        ).ConfigureAwait(false);
    }

    protected override async Task ReactivateScheduleCoreAsync(long id)
    {
        DynamicParameters param = new();
        param.Add("@ID", id);

        await Db.NonQueryAsync("UPDATE Schedules SET IsActive = 1 WHERE ID = @ID;", param).ConfigureAwait(false);
    }

    protected override async Task<ScheduleEntity[]> SelectAllCoreAsync(bool includeInactive)
    {
        string sql = includeInactive
            ? "SELECT * FROM Schedules;"
            : "SELECT * FROM Schedules WHERE OneTime = 0 AND IsActive = 1;";

        return await Db.GetManyAsync<ScheduleEntity>(sql, parameters: null).ConfigureAwait(false);
    }

    protected override async Task<ScheduleEntity[]> SelectForWorkerCoreAsync(long workerID)
    {
        DynamicParameters param = new();
        param.Add("@WorkerID", workerID);

        return await Db.GetManyAsync<ScheduleEntity>(
            "SELECT * FROM Schedules WHERE OneTime = 0 AND WorkerID = @WorkerID AND IsActive = 1;",
            param
        ).ConfigureAwait(false);
    }

    protected override async Task<ScheduleEntity[]> SelectManyCoreAsync(long[] ids)
    {
        if (ids.Length == 0) { return Array.Empty<ScheduleEntity>(); }

        DynamicParameters param = new();
        param.Add("@IDs", ids);

        return await Db.GetManyAsync<ScheduleEntity>(
            "SELECT * FROM Schedules WHERE ID IN @IDs;", param).ConfigureAwait(false);
    }

    protected override async Task<ScheduleEntity> SelectCoreAsync(long id)
    {
        DynamicParameters param = new();
        param.Add("@ID", id);

        return await Db.GetOneAsync<ScheduleEntity>(
            "SELECT * FROM Schedules WHERE ID = @ID;", param).ConfigureAwait(false);
    }

    private static DynamicParameters BuildScheduleParams(
        bool sunday, bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
        TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC, TimeSpan? recurBetweenEndUTC)
    {
        DynamicParameters param = new();
        param.Add("@Sunday", sunday);
        param.Add("@Monday", monday);
        param.Add("@Tuesday", tuesday);
        param.Add("@Wednesday", wednesday);
        param.Add("@Thursday", thursday);
        param.Add("@Friday", friday);
        param.Add("@Saturday", saturday);
        param.Add("@TimeOfDayUTC", timeOfDayUTC);
        param.Add("@RecurTime", recurTime);
        param.Add("@RecurBetweenStartUTC", recurBetweenStartUTC);
        param.Add("@RecurBetweenEndUTC", recurBetweenEndUTC);
        return param;
    }
}
