namespace SimpleSchedulerDomainModels;

public record class JobWithWorker(
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
    Worker Worker
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
