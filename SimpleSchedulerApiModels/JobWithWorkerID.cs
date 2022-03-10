using System.Text.Json.Serialization;

namespace SimpleSchedulerApiModels;

public class JobWithWorkerID
    : Job
{
    public JobWithWorkerID() { }

    public JobWithWorkerID(long id, long scheduleID, DateTime insertDateUTC, DateTime queueDateUTC,
        DateTime? completeDateUTC, string statusCode, Guid acknowledgementCode,
        DateTime? acknowledgementDate, bool hasDetailedMessage, string? friendlyDuration,
        long workerID)
        : base(id, scheduleID, insertDateUTC, queueDateUTC,
            completeDateUTC, statusCode, acknowledgementCode,
            acknowledgementDate, hasDetailedMessage, friendlyDuration)
    {
        WorkerID = workerID;
    }

    [JsonPropertyName("wid")] public long WorkerID { get; set; }
}
