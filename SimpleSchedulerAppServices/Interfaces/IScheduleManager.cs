﻿using SimpleSchedulerModels;

namespace SimpleSchedulerAppServices.Interfaces;

public interface IScheduleManager
{
    Task DeactivateScheduleAsync(long id);
    Task ReactivateScheduleAsync(long id);
    Task<Schedule[]> GetSchedulesToInsertAsync();
    Task<Schedule[]> GetAllSchedulesAsync();
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
