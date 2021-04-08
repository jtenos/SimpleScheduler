﻿using System;

namespace SimpleSchedulerModels
{
    public record Schedule(long ScheduleID, bool IsActive, long WorkerID, bool Sunday, bool Monday,
        bool Tuesday, bool Wednesday, bool Thursday, bool Friday, bool Saturday,
        TimeSpan? TimeOfDayUTC, TimeSpan? RecurTime, TimeSpan? RecurBetweenStartUTC,
        TimeSpan? RecurBetweenEndUTC, bool OneTime);
}
