namespace SimpleSchedulerModels.ApiModels.Schedules;

public record class CreateScheduleRequest(
    long WorkerID,
    bool Sunday,
    bool Monday,
    bool Tuesday,
    bool Wednesday,
    bool Thursday,
    bool Friday,
    bool Saturday,
    TimeSpan? TimeOfDayUTC,
    TimeSpan? RecurTime,
    TimeSpan? RecurBetweenStartUTC,
    TimeSpan? RecurBetweenEndUTC
);
public record class CreateScheduleResponse();
