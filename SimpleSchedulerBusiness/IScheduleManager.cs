using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using SimpleSchedulerEntities;
using SimpleSchedulerModels;

namespace SimpleSchedulerBusiness
{
    public interface IScheduleManager
    {
        Task DeactivateScheduleAsync(long scheduleID, CancellationToken cancellationToken);
        Task ReactivateScheduleAsync(long scheduleID, CancellationToken cancellationToken);
        Task<ImmutableArray<ScheduleDetail>> GetSchedulesToInsertAsync(CancellationToken cancellationToken);
        Task<ImmutableArray<ScheduleDetail>> GetAllSchedulesAsync(CancellationToken cancellationToken,
            bool getActive = true, bool getInactive = false);
        Task<ScheduleDetail> GetScheduleAsync(long scheduleID, CancellationToken cancellationToken);
        Task<long> AddScheduleAsync(long workerID, bool isActive, bool sunday,
            bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
            TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC,
            TimeSpan? recurBetweenEndUTC, bool oneTime, CancellationToken cancellationToken);
        Task UpdateScheduleAsync(long scheduleID, long workerID, bool sunday,
            bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
            TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC,
            TimeSpan? recurBetweenEndUTC, CancellationToken cancellationToken);
        Schedule ConvertToSchedule(ScheduleEntity entity);
    }
}
