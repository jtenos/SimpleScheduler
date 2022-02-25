namespace SimpleSchedulerDataEntities;

public record class JobEntity(
    long ID,
    long ScheduleID,
    DateTime InsertDateUTC,
    DateTime QueueDateUTC,
    DateTime? CompleteDateUTC,
    string StatusCode,
    Guid AcknowledgementCode,
    DateTime AcknowledgementDate,
    bool HasDetailedMessage
);
