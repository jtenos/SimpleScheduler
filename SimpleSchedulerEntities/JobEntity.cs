namespace SimpleSchedulerEntities
{
    public record JobEntity(
        long JobID,
        long ScheduleID,
        long InsertDateUTC,
        long QueueDateUTC,
        long? CompleteDateUTC,
        string StatusCode,
        string? DetailedMessage,
        string AcknowledgementID,
        long? AcknowledgementDate
    );
}
