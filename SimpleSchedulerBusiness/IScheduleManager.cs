using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using SimpleSchedulerModels;

namespace SimpleSchedulerBusiness
{
    public interface IScheduleManager
    {
        Task DeactivateScheduleAsync(int scheduleID, CancellationToken cancellationToken);
        Task ReactivateScheduleAsync(int scheduleID, CancellationToken cancellationToken);
        Task<ImmutableArray<ScheduleDetail>> GetSchedulesToInsertAsync(CancellationToken cancellationToken);
        Task<ImmutableArray<Schedule>> GetAllSchedulesAsync(CancellationToken cancellationToken,
            bool getActive = true, bool getInactive = false);
        Task<ScheduleDetail> GetScheduleAsync(int scheduleID, CancellationToken cancellationToken);
        Task<int> AddScheduleAsync(int workerID, bool isActive, bool sunday,
            bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
            TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC,
            TimeSpan? recurBetweenEndUTC, bool oneTime, CancellationToken cancellationToken);
        Task UpdateScheduleAsync(int scheduleID, int workerID, bool sunday,
            bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
            TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC,
            TimeSpan? recurBetweenEndUTC, CancellationToken cancellationToken);
    }
}
