using System.Text.Json.Serialization;

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

    [JsonPropertyName("id")] public long ID { get; set; }
    [JsonPropertyName("sid")] public long ScheduleID { get; set; }
    [JsonPropertyName("insDt")] public DateTime InsertDateUTC { get; set; }
    [JsonPropertyName("queDt")] public DateTime QueueDateUTC { get; set; }
    [JsonPropertyName("compDt")] public DateTime? CompleteDateUTC { get; set; }
    [JsonPropertyName("stat")] public string StatusCode { get; set; } = default!;
    [JsonPropertyName("ackCd")] public Guid AcknowledgementCode { get; set; }
    [JsonPropertyName("ackDt")] public DateTime? AcknowledgementDate { get; set; }
    [JsonPropertyName("hasMsg")] public bool HasDetailedMessage { get; set; }
    [JsonPropertyName("dur")] public string? FriendlyDuration { get; set; }
}
