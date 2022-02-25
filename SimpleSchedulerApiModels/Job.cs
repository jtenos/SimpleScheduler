using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels;

[DataContract]
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

    [DataMember(Order = 1)] public long ID { get; set; }
    [DataMember(Order = 2)] public long ScheduleID { get; set; }
    [DataMember(Order = 3)] public DateTime InsertDateUTC { get; set; }
    [DataMember(Order = 4)] public DateTime QueueDateUTC { get; set; }
    [DataMember(Order = 5)] public DateTime? CompleteDateUTC { get; set; }
    [DataMember(Order = 6)] public string StatusCode { get; set; } = default!;
    [DataMember(Order = 7)] public Guid AcknowledgementCode { get; set; } = default!;
    [DataMember(Order = 8)] public DateTime? AcknowledgementDate { get; set; }
    [DataMember(Order = 9)] public bool HasDetailedMessage { get; set; }
    [DataMember(Order = 10)] public string? FriendlyDuration { get; set; }
}
