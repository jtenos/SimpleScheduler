using System.Text.Json.Serialization;

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

    [JsonPropertyName("id")] public long ID { get; set; }
    [JsonPropertyName("active")] public bool IsActive { get; set; }
    [JsonPropertyName("wid")] public long WorkerID { get; set; }
    [JsonPropertyName("sun")] public bool Sunday { get; set; }
    [JsonPropertyName("mon")] public bool Monday { get; set; }
    [JsonPropertyName("tue")] public bool Tuesday { get; set; }
    [JsonPropertyName("wed")] public bool Wednesday { get; set; }
    [JsonPropertyName("thu")] public bool Thursday { get; set; }
    [JsonPropertyName("fri")] public bool Friday { get; set; }
    [JsonPropertyName("sat")] public bool Saturday { get; set; }
    [JsonPropertyName("tmOfDay")] public TimeSpan? TimeOfDayUTC { get; set; }
    [JsonPropertyName("recurTm")] public TimeSpan? RecurTime { get; set; }
    [JsonPropertyName("recStart")] public TimeSpan? RecurBetweenStartUTC { get; set; }
    [JsonPropertyName("recEnd")] public TimeSpan? RecurBetweenEndUTC { get; set; }
    [JsonPropertyName("oneTime")] public bool OneTime { get; set; }

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
