namespace SimpleSchedulerModels;

public record class Schedule(
    long ID, 
    bool IsActive, 
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
    TimeSpan? RecurBetweenEndUTC,
    bool OneTime
);
