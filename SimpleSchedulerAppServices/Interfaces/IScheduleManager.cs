using OneOf;
using OneOf.Types;
using SimpleSchedulerModels;

namespace SimpleSchedulerAppServices.Interfaces;

public interface IScheduleManager
{
    Task DeactivateScheduleAsync(long id);
    Task ReactivateScheduleAsync(long id);
    Task<Schedule[]> GetSchedulesToInsertAsync();
    Task<Schedule[]> GetAllSchedulesAsync();
    Task<Schedule[]> GetSchedulesForWorkerAsync(long workerID);
    Task<Schedule> GetScheduleAsync(long id);
    Task<OneOf<Success, Error<string>>> AddScheduleAsync(long workerID, 
        bool sunday, bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
        TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC,
        TimeSpan? recurBetweenEndUTC);
    Task<OneOf<Success, Error<string>>> UpdateScheduleAsync(long id, bool sunday,
        bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
        TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC,
        TimeSpan? recurBetweenEndUTC);
}
