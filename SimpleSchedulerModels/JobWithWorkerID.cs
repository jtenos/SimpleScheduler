namespace SimpleSchedulerModels;

public record class JobWithWorkerID(
    long ID,
    long ScheduleID,
    DateTime InsertDateUTC,
    DateTime QueueDateUTC,
    DateTime? CompleteDateUTC,
    string StatusCode,
    Guid AcknowledgementCode,
    DateTime? AcknowledgementDate,
    bool HasDetailedMessage,
    string? FriendlyDuration,
    long WorkerID
) : Job(
    ID,
    ScheduleID,
    InsertDateUTC,
    QueueDateUTC,
    CompleteDateUTC,
    StatusCode,
    AcknowledgementCode,
    AcknowledgementDate,
    HasDetailedMessage,
    FriendlyDuration
);
