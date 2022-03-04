namespace SimpleSchedulerApiModels.Request.Schedules;

public class CreateScheduleRequest
{
    public CreateScheduleRequest() { }

    public CreateScheduleRequest(
        long workerID,
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
    }

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
}
