using System;

namespace SimpleSchedulerModels
{
    public record Job(int JobID, int ScheduleID, DateTime InsertDateUTC, DateTime QueueDateUTC,
        DateTime? CompleteDateUTC, string StatusCode, string? DetailedMessage,
        Guid AcknowledgementID, DateTime? AcknowledgementDate);
}
