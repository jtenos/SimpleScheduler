using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerData;
using SimpleSchedulerDataEntities;
using SimpleSchedulerDomainModels;
using System.ComponentModel.DataAnnotations;

namespace SimpleSchedulerAppServices.Implementations;

/// <summary>
/// Database-agnostic logic for the schedule manager (input validation, model mapping). The
/// provider-specific data access lives in the abstract Core methods.
/// </summary>
public abstract class ScheduleManagerBase : IScheduleManager
{
    protected IDatabase Db { get; }

    protected ScheduleManagerBase(IDatabase db)
    {
        Db = db;
    }

    // ---- provider-specific data access ----
    protected abstract Task InsertScheduleCoreAsync(long workerID,
        bool sunday, bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
        TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC, TimeSpan? recurBetweenEndUTC);
    protected abstract Task UpdateScheduleCoreAsync(long id,
        bool sunday, bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
        TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC, TimeSpan? recurBetweenEndUTC);
    protected abstract Task DeactivateScheduleCoreAsync(long id);
    protected abstract Task ReactivateScheduleCoreAsync(long id);
    protected abstract Task<ScheduleEntity[]> SelectAllCoreAsync(bool includeInactive);
    protected abstract Task<ScheduleEntity[]> SelectForWorkerCoreAsync(long workerID);
    protected abstract Task<ScheduleEntity[]> SelectManyCoreAsync(long[] ids);
    protected abstract Task<ScheduleEntity> SelectCoreAsync(long id);

    // ---- agnostic orchestration ----
    async Task IScheduleManager.AddScheduleAsync(long workerID,
        bool sunday, bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
        TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC, TimeSpan? recurBetweenEndUTC)
    {
        ValidateScheduleInput(sunday, monday, tuesday, wednesday, thursday, friday, saturday,
            timeOfDayUTC, recurTime, recurBetweenStartUTC, recurBetweenEndUTC);

        await InsertScheduleCoreAsync(workerID, sunday, monday, tuesday, wednesday, thursday, friday, saturday,
            timeOfDayUTC, recurTime, recurBetweenStartUTC, recurBetweenEndUTC).ConfigureAwait(false);
    }

    async Task IScheduleManager.UpdateScheduleAsync(long id,
        bool sunday, bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
        TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC, TimeSpan? recurBetweenEndUTC)
    {
        ValidateScheduleInput(sunday, monday, tuesday, wednesday, thursday, friday, saturday,
            timeOfDayUTC, recurTime, recurBetweenStartUTC, recurBetweenEndUTC);

        await UpdateScheduleCoreAsync(id, sunday, monday, tuesday, wednesday, thursday, friday, saturday,
            timeOfDayUTC, recurTime, recurBetweenStartUTC, recurBetweenEndUTC).ConfigureAwait(false);
    }

    async Task IScheduleManager.DeactivateScheduleAsync(long id)
        => await DeactivateScheduleCoreAsync(id).ConfigureAwait(false);

    async Task IScheduleManager.ReactivateScheduleAsync(long id)
        => await ReactivateScheduleCoreAsync(id).ConfigureAwait(false);

    async Task<Schedule[]> IScheduleManager.GetAllSchedulesAsync()
        => (await SelectAllCoreAsync(includeInactive: false).ConfigureAwait(false))
            .Select(ModelBuilders.GetSchedule).ToArray();

    async Task<Schedule[]> IScheduleManager.GetAllSchedulesIncludingInactiveAsync()
        => (await SelectAllCoreAsync(includeInactive: true).ConfigureAwait(false))
            .Select(ModelBuilders.GetSchedule).ToArray();

    async Task<Schedule[]> IScheduleManager.GetSchedulesForWorkerAsync(long workerID)
        => (await SelectForWorkerCoreAsync(workerID).ConfigureAwait(false))
            .Select(ModelBuilders.GetSchedule).ToArray();

    async Task<Schedule[]> IScheduleManager.GetSchedulesAsync(long[] ids)
        => (await SelectManyCoreAsync(ids).ConfigureAwait(false))
            .Select(ModelBuilders.GetSchedule).ToArray();

    async Task<Schedule> IScheduleManager.GetScheduleAsync(long id)
        => ModelBuilders.GetSchedule(await SelectCoreAsync(id).ConfigureAwait(false));

    private static void ValidateScheduleInput(
        bool sunday, bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
        TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC, TimeSpan? recurBetweenEndUTC)
    {
        if (!sunday && !monday && !tuesday && !wednesday && !thursday && !friday && !saturday)
        {
            throw new ValidationException("You must select at least one day.");
        }
        if (timeOfDayUTC.HasValue && recurTime.HasValue)
        {
            throw new ValidationException("You must select only one of TimeOfDay/RecurTime");
        }
        if (!timeOfDayUTC.HasValue && !recurTime.HasValue)
        {
            throw new ValidationException("You must select one of TimeOfDay/RecurTime");
        }
        if (recurBetweenStartUTC.HasValue && recurBetweenEndUTC.HasValue && recurBetweenStartUTC > recurBetweenEndUTC)
        {
            throw new ValidationException("Recur between times invalid");
        }
    }
}
