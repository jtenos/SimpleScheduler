using System.Collections.Immutable;
using SimpleSchedulerEntities;
using SimpleSchedulerModels;

namespace SimpleSchedulerAppServices.Interfaces;

public interface IScheduleManager
{
    Task DeactivateScheduleAsync(long scheduleID, CancellationToken cancellationToken);
    Task ReactivateScheduleAsync(long scheduleID, CancellationToken cancellationToken);
    Task<ImmutableArray<ScheduleDetail>> GetSchedulesToInsertAsync(CancellationToken cancellationToken);
    Task<ImmutableArray<ScheduleDetail>> GetAllSchedulesAsync(bool getActive, bool getInactive,
        bool getOneTime, CancellationToken cancellationToken);
    Task<ScheduleDetail> GetScheduleAsync(long scheduleID, CancellationToken cancellationToken);
    Task AddScheduleAsync(long workerID, 
        bool sunday, bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
        TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC,
        TimeSpan? recurBetweenEndUTC, CancellationToken cancellationToken);
    Task UpdateScheduleAsync(long scheduleID, bool sunday,
        bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
        TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC,
        TimeSpan? recurBetweenEndUTC, CancellationToken cancellationToken);
    Schedule ConvertToSchedule(ScheduleEntity entity);
}
