namespace SimpleSchedulerDataEntities;

public record class JobWithWorkerIDEntity(
    long ID,
    long ScheduleID,
    DateTime InsertDateUTC,
    DateTime QueueDateUTC,
    DateTime? CompleteDateUTC,
    string StatusCode,
    Guid AcknowledgementCode,
    DateTime? AcknowledgementDate,
    bool HasDetailedMessage,
    long WorkerID,
    string WorkerName
) : JobEntity(
    ID,
    ScheduleID,
    InsertDateUTC,
    QueueDateUTC,
    CompleteDateUTC,
    StatusCode,
    AcknowledgementCode,
    AcknowledgementDate,
    HasDetailedMessage
);
