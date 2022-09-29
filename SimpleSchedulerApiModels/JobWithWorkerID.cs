namespace SimpleSchedulerApiModels;

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
    long WorkerID,
    string WorkerName
): Job(
    ID: ID,
    ScheduleID: ScheduleID,
    InsertDateUTC: InsertDateUTC,
    QueueDateUTC: QueueDateUTC,
    CompleteDateUTC: CompleteDateUTC,
    StatusCode: StatusCode,
    AcknowledgementCode: AcknowledgementCode,
    AcknowledgementDate: AcknowledgementDate,
    HasDetailedMessage: HasDetailedMessage,
    FriendlyDuration: FriendlyDuration
);
