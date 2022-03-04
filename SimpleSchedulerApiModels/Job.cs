namespace SimpleSchedulerApiModels;

public class Job
{
    public Job() { }

    public Job(long id, long scheduleID, DateTime insertDateUTC, DateTime queueDateUTC,
        DateTime? completeDateUTC, string statusCode, Guid acknowledgementCode,
        DateTime? acknowledgementDate, bool hasDetailedMessage, string? friendlyDuration)
    {
        ID = id;
        ScheduleID = scheduleID;
        InsertDateUTC = insertDateUTC;
        QueueDateUTC = queueDateUTC;
        CompleteDateUTC = completeDateUTC;
        StatusCode = statusCode;
        AcknowledgementCode = acknowledgementCode;
        AcknowledgementDate = acknowledgementDate;
        HasDetailedMessage = hasDetailedMessage;
        FriendlyDuration = friendlyDuration;
    }

    public long ID { get; set; }
    public long ScheduleID { get; set; }
    public DateTime InsertDateUTC { get; set; }
    public DateTime QueueDateUTC { get; set; }
    public DateTime? CompleteDateUTC { get; set; }
    public string StatusCode { get; set; } = default!;
    public Guid AcknowledgementCode { get; set; } = default!;
    public DateTime? AcknowledgementDate { get; set; }
    public bool HasDetailedMessage { get; set; }
    public string? FriendlyDuration { get; set; }
}
