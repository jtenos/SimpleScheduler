using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Request.Schedules;

[DataContract]
public class UpdateScheduleRequest
{
    public UpdateScheduleRequest() { }

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
        TimeSpan? recurBetweenEndUTC)
    {
        ID = id;
        Sunday = sunday;
        Monday = monday;
        Tuesday = tuesday;
        Wednesday = wednesday;
        Thursday = thursday;
        Friday = friday;
        Saturday = saturday;
        TimeOfDayUTC = timeOfDayUTC;
        RecurTime = recurTime;
        RecurBetweenStartUTC = recurBetweenStartUTC;
        RecurBetweenEndUTC = recurBetweenEndUTC;
    }

    [DataMember(Order = 1)] public long ID { get; set; }
    [DataMember(Order = 2)] public bool Sunday { get; set; }
    [DataMember(Order = 3)] public bool Monday { get; set; }
    [DataMember(Order = 4)] public bool Tuesday { get; set; }
    [DataMember(Order = 5)] public bool Wednesday { get; set; }
    [DataMember(Order = 6)] public bool Thursday { get; set; }
    [DataMember(Order = 7)] public bool Friday { get; set; }
    [DataMember(Order = 8)] public bool Saturday { get; set; }
    [DataMember(Order = 9)] public TimeSpan? TimeOfDayUTC { get; set; }
    [DataMember(Order = 10)] public TimeSpan? RecurTime { get; set; }
    [DataMember(Order = 11)] public TimeSpan? RecurBetweenStartUTC { get; set; }
    [DataMember(Order = 12)] public TimeSpan? RecurBetweenEndUTC { get; set; }
}
