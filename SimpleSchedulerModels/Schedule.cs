using System;

namespace SimpleSchedulerModels
{
    public record Schedule(long ScheduleID, bool IsActive, long WorkerID, bool Sunday, bool Monday,
        bool Tuesday, bool Wednesday, bool Thursday, bool Friday, bool Saturday,
        SimpleTimeSpan? TimeOfDayUTC, SimpleTimeSpan? RecurTime,
        SimpleTimeSpan? RecurBetweenStartUTC,
        SimpleTimeSpan? RecurBetweenEndUTC, bool OneTime);
}
