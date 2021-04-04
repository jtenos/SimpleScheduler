namespace SimpleSchedulerEntities
{
    public record LoginAttemptEntity(
        long LoginAttemptID,
        long SubmitDateUTC,
        string EmailAddress,
        string ValidationKey,
        long ValidationDateUTC
    );
}
