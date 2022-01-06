using System;

namespace SimpleSchedulerModels
{
    public record Job(long JobID, long ScheduleID, DateTime InsertDateUTC, DateTime QueueDateUTC,
        DateTime? CompleteDateUTC, string StatusCode, string? DetailedMessage, long DetailedMessageSize,
        string AcknowledgementID, DateTime? AcknowledgementDate, string? FriendlyDuration);
}
