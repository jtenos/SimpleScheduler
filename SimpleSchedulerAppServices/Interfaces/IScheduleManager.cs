using SimpleSchedulerDomainModels;

namespace SimpleSchedulerAppServices.Interfaces;

public interface IScheduleManager
{
    Task DeactivateScheduleAsync(long id);
    Task ReactivateScheduleAsync(long id);
    Task<Schedule[]> GetAllSchedulesAsync();
    Task<Schedule[]> GetAllSchedulesIncludingInactiveAsync();
    Task<Schedule[]> GetSchedulesForWorkerAsync(long workerID);
    Task<Schedule[]> GetSchedulesAsync(long[] ids);
    Task<Schedule> GetScheduleAsync(long id);
    Task AddScheduleAsync(long workerID, 
        bool sunday, bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
        TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC,
        TimeSpan? recurBetweenEndUTC);
    Task UpdateScheduleAsync(long id, bool sunday,
        bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
        TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC,
        TimeSpan? recurBetweenEndUTC);
}
