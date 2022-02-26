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

    public string GetFormatted()
    {
        string days;

        if (Sunday && Monday && Tuesday && Wednesday && Thursday && Friday && Saturday)
        {
            days = "Every day";
        }
        else if (!Sunday && Monday && Tuesday && Wednesday && Thursday && Friday && !Saturday)
        {
            days = "Weekdays";
        }
        else if (Sunday && !Monday && !Tuesday && !Wednesday && !Thursday && !Friday && Saturday)
        {
            days = "Weekends";
        }
        else
        {
            days = string.Join(' ', new[] {
                Sunday ? "Su" : "__",
                Monday ? "Mo" : "__",
                Tuesday ? "Tu" : "__",
                Wednesday ? "We" : "__",
                Thursday ? "Th" : "__",
                Friday ? "Fr" : "__",
                Saturday ? "Sa" : "__"
            });
        }

        string times = "Unknown";
        const string TS_FORMAT = "hh:mm";
        if (TimeOfDayUTC.HasValue)
        {
            times = $"at { TimeOfDayUTC.Value.ToString(TS_FORMAT) }";
        }
        else if (RecurTime.HasValue)
        {
            times = GetFormattedTimeSpan(RecurTime.Value);
        }

        if (RecurBetweenStartUTC.HasValue && RecurBetweenEndUTC.HasValue)
        {
            times += $" between { RecurBetweenStartUTC.Value.ToString(TS_FORMAT) } and { RecurBetweenEndUTC.Value.ToString(TS_FORMAT) }";
        }
        else if (RecurBetweenStartUTC.HasValue)
        {
            times += $" starting at { RecurBetweenStartUTC.Value.ToString(TS_FORMAT) }";
        }
        else if (RecurBetweenEndUTC.HasValue)
        {
            times += $" until { RecurBetweenEndUTC.Value.ToString(TS_FORMAT) }";
        }

        return $"{ days } [{ times }]";
    }

    private static string GetFormattedTimeSpan(TimeSpan ts)
    {
        if (ts.Hours == 1) { return "every hour"; }
        if (ts.Hours > 1) { return $"every { ts.Hours } hours"; }
        if (ts.Minutes == 1) { return "every minute"; }
        if (ts.Minutes > 1) { return $"every { ts.Minutes} minutes"; }
        return "unknown";
    }
}
