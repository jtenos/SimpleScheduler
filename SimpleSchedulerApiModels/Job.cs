namespace SimpleSchedulerApiModels;

public record class Job(
    long ID, 
    long ScheduleID, 
    DateTime InsertDateUTC, 
    DateTime QueueDateUTC,
    DateTime? CompleteDateUTC, 
    string StatusCode, 
    Guid AcknowledgementCode,
    DateTime? AcknowledgementDate, 
    bool HasDetailedMessage, 
    string? FriendlyDuration
);
