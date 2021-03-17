using System;

namespace SimpleSchedulerModels
{
    public record Schedule(int ScheduleID, bool IsActive, int WorkerID, bool Sunday, bool Monday,
        bool Tuesday, bool Wednesday, bool Thursday, bool Friday, bool Saturday, TimeSpan? TimeOfDayUTC,
        TimeSpan? RecurTime, TimeSpan? RecurBetweenStartUTC, TimeSpan? RecurBetweenEndUTC, bool OneTime);
}
