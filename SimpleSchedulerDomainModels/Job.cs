namespace SimpleSchedulerDomainModels;

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
)
{
    public const string STATUS_RUNNING = "RUN";
    public const string STATUS_ERROR = "ERR";
    public const string STATUS_NEW = "NEW";
}
