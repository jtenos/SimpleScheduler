using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels;

[DataContract]
public class Schedule
{
    public Schedule() { }

    public Schedule(long id, bool isActive, long workerID,
        bool sunday, bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday,
        TimeSpan? timeOfDayUTC, TimeSpan? recurTime, TimeSpan? recurBetweenStartUTC, TimeSpan? recurBetweenEndUTC,
        bool oneTime
    )
    {
        ID = id;
        IsActive = isActive;
        WorkerID = workerID;
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
        OneTime = oneTime;
    }

    [DataMember(Order = 1)] public long ID { get; set; }
    [DataMember(Order = 2)] public bool IsActive { get; set; }
    [DataMember(Order = 3)] public long WorkerID { get; set; }
    [DataMember(Order = 4)] public bool Sunday { get; set; }
    [DataMember(Order = 5)] public bool Monday { get; set; }
    [DataMember(Order = 6)] public bool Tuesday { get; set; }
    [DataMember(Order = 7)] public bool Wednesday { get; set; }
    [DataMember(Order = 8)] public bool Thursday { get; set; }
    [DataMember(Order = 9)] public bool Friday { get; set; }
    [DataMember(Order = 10)] public bool Saturday { get; set; }
    [DataMember(Order = 11)] public TimeSpan? TimeOfDayUTC { get; set; }
    [DataMember(Order = 12)] public TimeSpan? RecurTime { get; set; }
    [DataMember(Order = 13)] public TimeSpan? RecurBetweenStartUTC { get; set; }
    [DataMember(Order = 14)] public TimeSpan? RecurBetweenEndUTC { get; set; }
    [DataMember(Order = 15)] public bool OneTime { get; set; }
}
