using SimpleSchedulerBlazor.ProtocolBuffers.Types;

namespace SimpleSchedulerBlazor.ProtocolBuffers.Messages.Schedules;

partial class UpdateScheduleRequest
{
    public UpdateScheduleRequest(
        long id,
        bool sunday,
        bool monday,
        bool tuesday,
        bool wednesday,
        bool thursday,
        bool friday,
        bool saturday,
        TimeSpan? timeOfDayUTC,
        TimeSpan? recurTime,
        TimeSpan? recurBetweenStartUTC,
        TimeSpan? recurBetweenEndUTC
    )
    {
        ID = id;
        Sunday = sunday;
        Monday = monday;
        Tuesday = tuesday;
        Wednesday = wednesday;
        Thursday = thursday;
        Friday = friday;
        Saturday = saturday;
        TimeOfDayUTC = new NullableTimeSpan(timeOfDayUTC);
        RecurTime = new NullableTimeSpan(recurTime);
        RecurBetweenStartUTC = new NullableTimeSpan(recurBetweenStartUTC);
        RecurBetweenEndUTC = new NullableTimeSpan(recurBetweenEndUTC);
    }
}

partial class UpdateScheduleReply
{
}
