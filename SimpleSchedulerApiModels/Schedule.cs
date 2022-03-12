namespace SimpleSchedulerApiModels;

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

    public long ID { get; set; }
    public bool IsActive { get; set; }
    public long WorkerID { get; set; }
    public bool Sunday { get; set; }
    public bool Monday { get; set; }
    public bool Tuesday { get; set; }
    public bool Wednesday { get; set; }
    public bool Thursday { get; set; }
    public bool Friday { get; set; }
    public bool Saturday { get; set; }
    public TimeSpan? TimeOfDayUTC { get; set; }
    public TimeSpan? RecurTime { get; set; }
    public TimeSpan? RecurBetweenStartUTC { get; set; }
    public TimeSpan? RecurBetweenEndUTC { get; set; }
    public bool OneTime { get; set; }

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
        const string TS_FORMAT = @"hh\:mm";
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

    public static Schedule GetDummySchedule(long workerID)
    {
        return new(
            id: 0,
            isActive: false,
            workerID: workerID,
            sunday: true,
            monday: true,
            tuesday: true,
            wednesday: true,
            thursday: true,
            friday: true,
            saturday: true,
            timeOfDayUTC: null,
            recurTime: TimeSpan.FromHours(1),
            recurBetweenStartUTC: null,
            recurBetweenEndUTC: null,
            oneTime: false
        );
    }

    private static string GetFormattedTimeSpan(TimeSpan ts)
    {
        if (ts.Hours == 1 && ts.Minutes == 0) { return "every hour"; }
        if (ts.Hours > 1 && ts.Minutes == 0) { return $"every { ts.Hours } hours"; }
        if (ts.Hours > 0 && ts.Minutes > 0) { return $"every {ts:hh\\:mm}"; }
        if (ts.Minutes == 1) { return "every minute"; }
        if (ts.Minutes > 1) { return $"every { ts.Minutes} minutes"; }
        return "unknown";
    }
}
