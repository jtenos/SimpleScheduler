using SimpleSchedulerBlazor.ProtocolBuffers.Types;
using SimpleSchedulerModels;

namespace SimpleSchedulerBlazor.ProtocolBuffers.Messages;

partial class ScheduleMessage
{
    public ScheduleMessage(Schedule schedule)
    {
        ID = schedule.ID;
        IsActive = schedule.IsActive;
        WorkerID = schedule.WorkerID;
        Sunday = schedule.Sunday;
        Monday = schedule.Monday;
        Tuesday = schedule.Tuesday;
        Wednesday = schedule.Wednesday;
        Thursday = schedule.Thursday;
        Friday = schedule.Friday;
        Saturday = schedule.Saturday;
        TimeOfDayUTC = new NullableTimeSpan(schedule.TimeOfDayUTC);
        RecurTime = new NullableTimeSpan(schedule.RecurTime);
        RecurBetweenStartUTC = new NullableTimeSpan(schedule.RecurBetweenStartUTC);
        RecurBetweenEndUTC = new NullableTimeSpan(schedule.RecurBetweenEndUTC);
        OneTime = schedule.OneTime;
    }
}
