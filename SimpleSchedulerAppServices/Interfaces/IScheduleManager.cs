using System.Collections.Immutable;
using SimpleSchedulerModels;

namespace SimpleSchedulerAppServices.Interfaces;

public interface IScheduleManager
{
    Task DeactivateScheduleAsync(long id, CancellationToken cancellationToken);
    Task ReactivateScheduleAsync(long id, CancellationToken cancellationToken);
    Task<ImmutableArray<Schedule>> GetSchedulesToInsertAsync(CancellationToken cancellationToken);
    Task<ImmutableArray<Schedule>> GetAllSchedulesAsync(CancellationToken cancellationToken);
    Task<Schedule> GetScheduleAsync(long id, CancellationToken cancellationToken);
    Task AddScheduleAsync(long workerID, 
        bool sunday, bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
        TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC,
        TimeSpan? recurBetweenEndUTC, CancellationToken cancellationToken);
    Task UpdateScheduleAsync(long id, bool sunday,
        bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
        TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC,
        TimeSpan? recurBetweenEndUTC, CancellationToken cancellationToken);
}
